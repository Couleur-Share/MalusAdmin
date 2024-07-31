using MalusAdmin.Servers.Hub;
using MalusAdmin.Servers.SysRolePermission;
using MalusAdmin.Servers.SysUserButtonPermiss;
using MalusAdmin.WebApi.Filter;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using SqlSugar;

namespace MalusAdmin.WebApi;

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
                options.Filters.Add<RequestActionFilter>();
            })
            // ����Api��Ϊѡ��
            .ConfigureApiBehaviorOptions(options =>
            {
                // ����Ĭ��ģ����֤������
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddApiResult<CustomApiResultProvider>();


        // ����Jsonѡ��
        builder.Services.AddJsonOptions();

        // ���sqlsugar
        builder.Services.AddSqlsugarSetup();

        // ���swagger�ĵ�
        builder.Services.AddSwaggerSetup(); 

        // �Զ���ӷ����
        builder.Services.AddAutoServices("MalusAdmin.Servers");

        // ���jwt��֤
        //builder.Services.AddJwtSetup();

        //�����Ȩ
        //builder.Services.AddAuthorization();
        // ����Զ�����Ȩ
        builder.Services.AddAuthorizationSetup();
        // �滻Ĭ�� PermissionChecker
        //builder.Services.Replace(new ServiceDescriptor(typeof(IPermissionChecker), typeof(PermissionChecker), ServiceLifetime.Transient));
         
        // ��ӿ���֧��
        builder.Services.AddCorsSetup();

        //��Ӧ�����м��
        builder.Services.AddResponseCaching();

        //ʵʱӦ��
        builder.Services.AddSignalR();

        // ���EndpointsApiExplorer
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();

        //д�뾲̬�๩ȫ�ֻ�ȡ
        App.Instance = app.Services;
        App.Configuration = builder.Configuration;

        //ForwardedHeaders�м�����Զ��ѷ�����������ת��������X-Forwarded-For���ͻ�����ʵIP���Լ�X-Forwarded-Proto���ͻ��������Э�飩
        //�Զ���䵽HttpContext.Connection.RemoteIPAddress��HttpContext.Request.Scheme�У�����Ӧ�ô����ж�ȡ���ľ�����ʵ��IP����ʵ��Э���ˣ�����ҪӦ�������⴦��
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });


        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // ��ȡ IApiDescriptionGroupCollectionProvider ����
                var provider = App.GetService<IApiDescriptionGroupCollectionProvider>();

                // ��������API������
                foreach (var descriptionGroup in provider.ApiDescriptionGroups.Items)
                {
                    // Ϊÿ������ָ��Swagger�ĵ��ͱ���
                    c.SwaggerEndpoint($"/swagger/{descriptionGroup.GroupName}/swagger.json", descriptionGroup.GroupName);
                }
                //ָ��Swagger JSON�ļ����ս�㣬���ڼ��غ���ʾAPI�ĵ���
                // ΪĬ�Ϸ������ö˵�
                c.SwaggerEndpoint("/swagger/vdefault/swagger.json", "Default API");
                //ָ��swagger�ĵ�������Ŀ¼ ��Ĭ��Ϊswagger
                //����ͨ������Ϊ���ַ�������Swagger UIֱ���ڸ�·���½��з���
                //c.RoutePrefix = string.Empty;

                //����Ĭ�ϵĽӿ��ĵ�չ����ʽ����ѡֵ����None��List��Full��
                //Ĭ��ֵΪNone����ʾ��չ���ӿ��ĵ���
                //List��ʾֻչ���ӿ��б�
                //Full��ʾչ�����нӿ�����
                c.DocExpansion(DocExpansion.None); // ����Ϊ����ģʽ 
                c.DisplayRequestDuration();
                c.EnablePersistAuthorization();

                //c.UseRequestInterceptor("(request) => { return defaultRequestInterceptor(request); }");
                //c.UseResponseInterceptor("(response) => { return defaultResponseInterceptor(response); }");
            });
        }


        app.UseHttpsRedirection(); // ����ǰ�棬ȷ����������ͨ��HTTPS

        app.UseRouting(); // ȷ��·��

        app.UseCors(); // ���ÿ�����Դ����

        app.UseMiddleware<CheckToken>(); // ���CheckToken�������֤�м����������֤֮ǰ

        app.UseAuthentication(); // ���������֤�м��

        app.UseAuthorization(); // ������Ȩ�м��

        app.UseResponseCaching(); // Ӧ����Ӧ����

        app.UseDefaultFiles(); // �ṩĬ���ļ�֧��
        app.UseStaticFiles(); // ���þ�̬�ļ�����

        app.MapHub<OnlineUserHub>("/hub"); // ӳ��SignalR Hub

        app.MapControllers(); // ӳ�������


        app.Run(); // ����������
    }
}