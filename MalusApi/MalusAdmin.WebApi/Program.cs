using MalusAdmin.Common.Components.Token;
using MalusAdmin.Servers;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace MalusAdmin.WebApi
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
                options.Filters.Add<RequestActionFilter>();
            }) 
            // ����Api��Ϊѡ��
            .ConfigureApiBehaviorOptions(options =>
            {
                // ����Ĭ��ģ����֤������
                options.SuppressModelStateInvalidFilter = true;
            });
             

            // ����Jsonѡ��
            builder.Services.AddJsonOptions();

            // ���sqlsugar
            builder.Services.AddSqlsugarSetup();

            // ���swagger�ĵ�
            builder.Services.AddSwaggerSetup();

            // ��ӻ�������
            //builder.Services.AddBaseServicesSetup();

            // �Զ���ӷ����
            builder.Services.AddAutoServices("MalusAdmin.Servers");

            // ���jwt��֤
            //builder.Services.AddJwtSetup();
            // ����Զ�����Ȩ
            builder.Services.AddAuthorizationSetup();
            // �滻Ĭ�� PermissionChecker
            //builder.Services.Replace(new ServiceDescriptor(typeof(IPermissionChecker), typeof(PermissionChecker), ServiceLifetime.Transient));

            //Token
            //�ṩ�˷��ʵ�ǰHTTP�����ģ�HttpContext���ķ���
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddSingleton<ITokenService, GuidTokenService>(); 
            builder.Services.AddSingleton<IBaseService, BaseService>(); 

            // ��ӿ���֧��
            builder.Services.AddCorsSetup();
            // ���EndpointsApiExplorer
            builder.Services.AddEndpointsApiExplorer(); 

            var app = builder.Build();
             
            //д�뾲̬�๩ȫ�ֻ�ȡ
            App.Instance = app.Services;
            App.Configuration = builder.Configuration;

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    //ָ��Swagger JSON�ļ����ս�㣬���ڼ��غ���ʾAPI�ĵ���
                    //��Ҫ�ṩJSON�ļ���URL��һ����ʶ�������
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
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
             
            //Token��֤
            //app.UseMiddleware<CheckToken>();
             
            app.UseHttpsRedirection();

            app.UseRouting();
            // UseCors ������ UseRouting ֮��UseResponseCaching��UseAuthorization ֮ǰ
            app.UseCors();

            // ʹ�������֤
            app.UseAuthentication();

            // Ȼ������Ȩ�м��
            app.UseAuthorization();

            //ʹ�þ�̬�ļ�
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }
    }
}
