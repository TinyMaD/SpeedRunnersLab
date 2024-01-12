using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SpeedRunners
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging((context, loggingBuilder) =>
                {
                    // 过滤一些系统默认日志
                    loggingBuilder.AddFilter("System", LogLevel.Error);
                    loggingBuilder.AddFilter("Microsoft", LogLevel.Error);
                    loggingBuilder.AddLog4Net();// 配置文件,如果名字叫log4net.config，在根目录则可以不指定路径
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseIISIntegration();// IIS
                    //.UseKestrel((context, options) =>
                    //{
                    //    options.Configure(context.Configuration.GetSection("Kestrel"));
                    //});// 控制台
                });
    }
}
