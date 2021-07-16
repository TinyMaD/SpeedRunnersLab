using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            // 处理跨域
            services.AddCors(options =>
            {
                options.AddPolicy("default",
                builder => builder.WithOrigins(Configuration.GetSection($"AllowedHosts:{Env.EnvironmentName}").Value.ToString()).AllowAnyHeader().AllowAnyMethod());
            });
            // 注册全局Configuration
            services.AddSingleton(new AppSettings(Configuration));
            services.AddControllers(
                options =>
                {
                    options.Filters.Add<GlobalExceptionsFilter>();// 全局异常捕获
                    options.Filters.Add<ResponseFilter>();// 统一处理出参
                })
                .AddNewtonsoftJson();
            // 批量注册BLL服务
            services.AddAllBLL();
            // 注册当前用户信息
            services.AddScoped<MUser>();
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
