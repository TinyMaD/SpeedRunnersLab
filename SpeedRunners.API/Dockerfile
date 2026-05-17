# 多阶段构建：build 阶段 publish，runtime 阶段只带运行时
# CI 构建命令：docker build -t srlab-api ./SpeedRunners.API

# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

# 先只复制 csproj，restore 一次就缓存住 NuGet
COPY SpeedRunners/SpeedRunners.csproj            SpeedRunners/
COPY SpeedRunners.BLL/SpeedRunners.BLL.csproj    SpeedRunners.BLL/
COPY SpeedRunners.DAL/SpeedRunners.DAL.csproj    SpeedRunners.DAL/
COPY SpeedRunners.Model/SpeedRunners.Model.csproj SpeedRunners.Model/
COPY SpeedRunners.Utils/SpeedRunners.Utils.csproj SpeedRunners.Utils/
RUN dotnet restore SpeedRunners/SpeedRunners.csproj

# 再复制其余源码并 publish
COPY . .
RUN dotnet publish SpeedRunners/SpeedRunners.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS runtime
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SpeedRunners.dll"]
