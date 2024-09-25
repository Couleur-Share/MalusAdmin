﻿using MalusAdmin.Common.Components.Token;
using MalusAdmin.Servers;
using MalusAdmin.Servers.SysRolePermission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using static System.Net.Mime.MediaTypeNames;

namespace MalusAdmin.WebApi;

public class CheckToken
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CheckToken(RequestDelegate next,
        IServiceScopeFactory serviceScopeFactory,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
    {
        _next = next;
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 获取当前请求的Endpoint
        var endpoint = context.GetEndpoint();

        //一些禁用的资源放行检查
        var DisplayNameStr = new string[] {
            "hub"//
        };

        // 检查Endpoint的元数据中是否包含AllowAnonymous特性 
        var hasAllowAnonymous = endpoint.Metadata
            .OfType<IAllowAnonymous>()
            .Any();

        //静态资源直接放行 
        //禁用检查的资源放行
        //匿名访问的资源直接放行
        if (endpoint is null|| DisplayNameStr.Contains(endpoint.DisplayName)|| hasAllowAnonymous)
        {
            await _next(context);
            return;
        }
           
        //权限检查
        var token = context.Request.Query["token"].ToString();

        //当前token是否在缓存中
        var tokenService = App.GetService<ITokenService>();

        //是否过期
        var validataToken =await tokenService.ValidateToken(token);
        if (validataToken)
        {
            await Res401Async(context);
            return;
        }

        //解析是否正确
        using (var scope = _serviceScopeFactory.CreateScope())
        {
             
            //刷新用户的token过期时间
            await tokenService.RefreshTokenAsync(token);

            var User =await tokenService.ParseTokenAsync(token);
            if (User is null)
            {
                await Res401Async(context);
                return;
            }
            // 权限校验  把 User.UserId!=拿掉就所有人进行权限校验
            if (endpoint is RouteEndpoint routeEndpoint && !User.IsSuperAdmin)
            {
                // 获取路由模式
                //.Split("{")[0] 处理路由 api:SysUser:Delete:{id} 
                var routePattern = routeEndpoint.RoutePattern.RawText?.Replace('/', ':').Split("{")[0];
                // 处理最后 ':' 的位置
                int lastColonIndex = routePattern.LastIndexOf(':');
                if (lastColonIndex != -1)
                {
                    // 删除最后一个 ':' 后面的所有内容
                    routePattern = routePattern.Substring(0, lastColonIndex);
                }
                var rolePermissService = scope.ServiceProvider.GetRequiredService<ISysRolePermission>();

                var hapermissableAllowAnonymous = endpoint?.Metadata
                    .OfType<PermissionAttribute>()
                    .Any() ?? true;
                if (hapermissableAllowAnonymous)
                {
                    // 权限检查  
                    if (!await rolePermissService.HavePermission(routePattern))
                    {
                        await Res403Async(context);
                        return;
                    }
                }

            }
        }
        

        await _next(context);
    }

    /// <summary>
    /// 登录后返回401
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Res401Async(HttpContext context)
    {
        var apiResult = new ApiResult(StatusCodes.Status401Unauthorized, "提供的令牌无效或已过期，请重新登录", "");
        // 设置响应的Content-Type为application/json
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json"; 
        await context.Response.WriteAsync(apiResult.ToJson(true)); 
    }

    /// <summary>
    /// 登录后返回暂无权限
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Res403Async(HttpContext context)
    { 
        var apiResult = new ApiResult(StatusCodes.Status207MultiStatus, "暂无权限", "");
       
        // 设置响应的Content-Type为application/json
        context.Response.ContentType = "application/json"; 
        // 写入JSON字符串到响应体
        await context.Response.WriteAsync(apiResult.ToJson(true));
    }
}