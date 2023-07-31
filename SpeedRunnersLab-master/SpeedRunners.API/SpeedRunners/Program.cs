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
                    // ����һЩϵͳĬ����־
                    loggingBuilder.AddFilter("System", LogLevel.Error);
                    loggingBuilder.AddFilter("Microsoft", LogLevel.Error);
                    loggingBuilder.AddLog4Net();// �����ļ�,������ֽ�log4net.config���ڸ�Ŀ¼����Բ�ָ��·��
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseUrls("https://0.0.0.0:888");// ��������˿ڸ�Ϊ888
                    webBuilder.UseStartup<Startup>()
                    .UseKestrel((context, options) =>
                    {
                        options.Configure(context.Configuration.GetSection("Kestrel"));
                    });
                });
    }
}
