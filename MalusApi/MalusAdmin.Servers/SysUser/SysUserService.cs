﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MalusAdmin.Common;
using MalusAdmin.Common.Components;
using MalusAdmin.Common.Components.Token;
using MalusAdmin.Encryption;
using MalusAdmin.Entity; 
using MalusAdmin.Servers.SysRoleMenu;
using MalusAdmin.Servers.SysUser;
using MalusAdmin.Servers.SysUser.Dto;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetTaste;
using SqlSugar; 

namespace MalusAdmin.Servers
{
    public class SysUserService: ISysUserService
    {  
        private readonly SqlSugarRepository<TSysUser> _sysUserRep;  // 仓储
        private readonly ITokenService _TokenService;
        private readonly IHttpContextAccessor _HttpContext;
        private readonly SysRoleMenuService _sysRoleMenuService;
        private readonly SysMenuService _sysMenuService;

        public SysUserService(SqlSugarRepository<TSysUser> sysUserRep,
            ITokenService tokenService, SysRoleMenuService sysRoleMenuService,
            SysMenuService sysMenuService,
            IHttpContextAccessor httpContext)
        {
            _sysUserRep = sysUserRep;
            _TokenService = tokenService;
            _HttpContext = httpContext;
            _sysRoleMenuService = sysRoleMenuService;
            _sysMenuService = sysMenuService;
        }

        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<SysUserLoginOut> Login(SysUserLoginIn input)
        {
            
            var user =await _sysUserRep
                .Where(t => t.Account.ToLower() == input.Account.ToLower()).FirstAsync();

            if (user.PassWord != Md5Util.Encrypt(input.PassWord).ToUpper())
            {  
              throw new Exception("密码输入错误");
            }
            if (user.Status != 1)
            {
                throw new Exception("该账户已被冻结"); 
            }


            TokenData tokenData = new TokenData
            {
                UserId = user.Id,
                UserAccount = user.Account,
                UserRolesId = user.UserRolesId,
            };

            _TokenService.RemoveCheckToken(tokenData.UserId);
            string UserToken = _TokenService.GenerateToken(_HttpContext.HttpContext, tokenData);
             

            return new SysUserLoginOut() { Id=user.Id,Name=user.Name,Token=UserToken };
        }
         
        /// <summary>
        /// 获取用户的信息
        /// </summary>
        /// <returns></returns>
       
        public async Task<GetUserInfoOut> GetUserInfo()
        {
           
            var user = await _sysUserRep
           .Where(t => t.Id == TokenInfo.User.UserId).FirstAsync();
            return new GetUserInfoOut() 
            { 
                userId= user.Id,
                userName= user.Name,
                roles =new List<string> {  },
                buttons=new List<string> {  }
            };
        }


        /// <summary>
        /// 用户列表分页
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<PageList<TSysUser>> PageList(UserPageIn input)
        {
            var dictTypes = await _sysUserRep.AsQueryable() 
                 .WhereIF(!string.IsNullOrWhiteSpace(input.KeyWord), u => u.Name.Contains(input.KeyWord.Trim()))
                 .WhereIF(input.Status!=null, u => u.Status==input.Status)
                 //.Select<UserPageOut>()
                 .ToPagedListAsync(input.PageNo, input.PageSize);
            return dictTypes.PagedResult();
        }

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <returns></returns> 
        public async Task<bool> Add(UserAddAndUpIn input)
        {
            var isExist = await _sysUserRep.Where(x => x.Account == input.Account).AnyAsync();
            if(isExist) ResultCode.Fail.JsonR("已存在当前账号");
            var entity = input.Adapt<TSysUser>();
            entity.PassWord= Md5Util.Encrypt(input.PassWord);
            return await _sysUserRep.InsertReturnIdentityAsync(entity)>0;  
        }



        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> Delete(int userId)
        {
            var entity = await _sysUserRep.FirstOrDefaultAsync(u => u.Id == userId);
            if (entity==null) ResultCode.Fail.JsonR("为找到当前账号");
            entity.SysIsDelete=true;
            return  await _sysUserRep.UpdateAsync(entity)>0;
        }

        /// <summary>
        /// 更新用户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<bool> Update(UserAddAndUpIn input)
        {
            var entity = await _sysUserRep.FirstOrDefaultAsync(u => u.Id == input.Id);
            if (entity == null) ResultCode.Fail.JsonR("为找到当前账号"); 

            var sysUser = input.Adapt<TSysUser>();
            return await _sysUserRep.AsUpdateable(sysUser).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync()>0;
        }


        /// <summary>
        /// 获取登录用户的菜单权限
        /// </summary>
        /// <returns></returns>
        public async Task<UserMenuOut> GetUserMenu()
        {
            var Out=new UserMenuOut();
            //获取所有的菜单权限
            var tree = await _sysMenuService.MenuTreeList();
            ////获取当前用户的菜单权限
            var menuid = await _sysRoleMenuService.RoleUserMenu(TokenInfo.User.UserRolesId);

            //当用户为1的时候，设置为超级管理官
            if (TokenInfo.User.UserId==1)
            {
                menuid = tree.Records.Select(x => x.Id).ToList();
            }

            var res =new List<UserMenu>();
            foreach (var item in tree.Records.Where(x=> menuid.Contains(x.Id)))
            {
                res.Add(ConvertMenu(item));
            }
            Out.Home = res.FirstOrDefault()?.Name;
            Out.Routes = res;

            return Out;
        }

        /// <summary>
        /// 私有方法，转化前端路由
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        private UserMenu ConvertMenu(TSysMenu menu)
        {
            return new UserMenu
            {
                Name = menu.RouteName,
                Path = menu.RoutePath,
                Component = menu.Component,
                Meta = new Meta
                { 
                    Title = menu.MenuName,
                    Icon = menu.Icon,
                    Order = menu.Sort, 
                },
                Children = menu.children?.Select(ConvertMenu).ToList()
            };
        }
    }
}
