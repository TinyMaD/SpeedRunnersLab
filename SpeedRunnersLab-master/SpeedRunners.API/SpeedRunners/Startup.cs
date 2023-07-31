using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SpeedRunners.Filter;
using SpeedRunners.Middleware;
using SpeedRunners.Model;
using SpeedRunners.Service;

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
            // �������
            services.AddCors(options =>
            {
                options.AddPolicy("default",
                builder => builder.WithOrigins(Configuration.GetSection($"AllowedHosts:{Env.EnvironmentName}").Value.ToString()).AllowAnyHeader().AllowAnyMethod());
            });
            // ע��ȫ��Configuration
            services.AddSingleton(new AppSettings(Configuration));
            services.AddControllers(
                options =>
                {
                    options.Filters.Add<GlobalExceptionsFilter>();// ȫ���쳣����
                    options.Filters.Add<ResponseFilter>();// ͳһ�������
                })
                .AddNewtonsoftJson();
            // ����ע��BLL����
            services.AddAllBLL();
            // ע�ᵱǰ�û���Ϣ
            services.AddScoped<MUser>();
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            //app.UseHsts();

            app.UseRouting();

            app.UseCors("default");

            app.UseSRLabTokenAuth();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
