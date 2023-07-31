# SpeedRunnersLab
> 这是一个极简的以SpeedRunners游戏为主题的站点。主要运用的框架及组件库： Vuetify + vue-admin-template + ASP.NET Core + Steam标识登录

[线上地址](https://www.speedrunners.cn)

> 被忽略的appsettings.json文件
```
{
    "Kestrel": {
        "Endpoints": {
            "Https": {
                "Url": "https://0.0.0.0:888",
                "Certificate": {
                    "Path": "C:\\Users\\***.pfx",
                    "Password": "***"
                }
            }
        }
    },
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft": "Warning",
            "Microsoft.Hosting.Lifetime": "Information"
        }
    },
    "AllowedHosts": {
        "Development": "http://localhost:9528",
        "Production": "http://localhost:9528"
    },
    "ConnectionString": "server=***;database=***;uid=***;pwd=***;",
    "ApiKey": "***",
    "AccessKey": "***",
    "SecretKey": "***",
    "Refresh": "5"
}
```
