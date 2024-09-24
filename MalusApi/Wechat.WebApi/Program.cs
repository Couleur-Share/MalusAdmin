using MalusAdmin.Common;
using MalusAdmin.Common.Components; 
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.SwaggerUI;
using Wechat.Servers;
using Wechat.WebApi.Filter;

namespace Wechat.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //  ��������ע�� | ��Ӿ�̬�ļ���ȡ(���ȼ��Ƚϸ�)
            AppSettings.AddConfigSteup(builder.Configuration);

            //����ѡ��ע��
            builder.Services.AddConfigureSetup(builder.Configuration);

            // ����
            builder.Services.AddCacheSetup();

            //HttpContext
            builder.Services.AddHttpContextAccessor();

            // ��ӹ�����
            builder.Services.AddControllers(options =>
            {
                // ȫ���쳣����
                options.Filters.Add<GlobalExceptionFilter>();
                // ��־������
                //options.Filters.Add<RequestActionFilter>();
            })
                // ����Api��Ϊѡ��
                .ConfigureApiBehaviorOptions(options =>
                {
                    // ����Ĭ��ģ����֤������
                    options.SuppressModelStateInvalidFilter = true;
                })
                .AddApiResult<CustomApiResultProvider>(); ;

            // ����Jsonѡ��
            builder.Services.AddJsonOptions();

            // ���sqlsugar
            builder.Services.AddSqlsugarSetup();

            // ���jwt��֤
            builder.Services.AddJwtSetup();

            // ���swagger�ĵ�
            builder.Services.AddSwaggerSetup();

            // �Զ���ӷ����
            builder.Services.AddAutoServices("Wechat.Servers");
            //�����Ȩ
            builder.Services.AddAuthorization();
            // ����Զ�����Ȩ
            builder.Services.AddAuthorizationSetup();

            // �滻Ĭ�� PermissionChecker
            //builder.Services.Replace(new ServiceDescriptor(typeof(IPermissionChecker), typeof(PermissionChecker), ServiceLifetime.Transient));
            builder.Services.AddSingleton<WeChatGetOpenId>();
            builder.Services.AddSingleton<GalleryServiceController>();
            //builder.Services.AddSingleton<ITokenService, TokenService>();
            //builder.Services.AddScoped<ISysRolePermission, SysRolePermissionService>();

            // ��ӿ���֧��
            builder.Services.AddCorsSetup();

            //��Ӧ�����м��
            builder.Services.AddResponseCaching();

            //ʵʱӦ��
            //builder.Services.AddSignalR();

            // ���EndpointsApiExplorer
            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            //д�뾲̬�๩ȫ�ֻ�ȡ
            App.ServiceProvider = app.Services;
            App.Configuration = builder.Configuration;

            //ForwardedHeaders�м�����Զ��ѷ�����������ת��������X-Forwarded-For���ͻ�����ʵIP���Լ�X-Forwarded-Proto���ͻ��������Э�飩
            //�Զ���䵽HttpContext.Connection.RemoteIPAddress��HttpContext.Request.Scheme�У�����Ӧ�ô����ж�ȡ���ľ�����ʵ��IP����ʵ��Э���ˣ�����ҪӦ�������⴦��
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (AppSettings.DisplaySwaggerDoc)
            {
                app.UseSwaggerExtension();
            }

            app.UseHttpsRedirection(); // ����ǰ�棬ȷ����������ͨ��HTTPS

            app.UseRouting(); // ȷ��·��

            app.UseCors(); // ���ÿ�����Դ����

            //app.UseMiddleware<CheckToken>(); // ���CheckToken�������֤�м����������֤֮ǰ

            app.UseAuthentication(); // ���������֤�м��

            app.UseAuthorization(); // ������Ȩ�м��

            app.UseResponseCaching(); // Ӧ����Ӧ����

            app.UseDefaultFiles(); // �ṩĬ���ļ�֧��
            app.UseStaticFiles(); // ���þ�̬�ļ�����

            //app.MapHub<OnlineUserHub>("/hub"); // ӳ��SignalR Hub

            app.MapControllers(); // ӳ�������

            app.Run(); // ����������
        }
    }
}
