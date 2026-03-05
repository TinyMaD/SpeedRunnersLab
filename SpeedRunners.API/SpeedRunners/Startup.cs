using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpeedRunners.BLL;
using SpeedRunners.Filter;
using SpeedRunners.Middleware;
using SpeedRunners.Model;
using SpeedRunners.Service;
using SpeedRunners.Utils;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;

namespace SpeedRunners
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Env = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ��������
            services.AddCors(options =>
            {
                options.AddPolicy("default",
                builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });
            // ע��ȫ��Configuration
            services.AddSingleton(new AppSettings(Configuration));
            services.AddControllers(
                options =>
                {
                    options.Filters.Add<GlobalExceptionsFilter>();// ȫ���쳣����
                    options.Filters.Add<ResponseFilter>();// ͳһ��������
                })
                .AddNewtonsoftJson();
            // ����ע��BLL����
            services.AddAllBLL();
            // 添加内存缓存（用于成就定义缓存）
            services.AddMemoryCache();
            // 注册成就定义缓存服务
            services.AddScoped<AchievementSchemaService>();
            services.AddScoped<MUser>();
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            // ���ػ�
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            // ���ô���
            HttpHelper.SetProxy();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCors("default");

            app.UseSRLabTokenAuth();

            app.UseHeaderRequestLocalization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
