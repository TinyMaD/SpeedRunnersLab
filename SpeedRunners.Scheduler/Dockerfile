# 多阶段构建：build 阶段 publish，runtime 阶段只带 .NET 运行时
# CI 构建命令：docker build -t srlab-scheduler ./SpeedRunners.Scheduler

# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src

COPY SpeedRunners.Scheduler.csproj .
RUN dotnet restore SpeedRunners.Scheduler.csproj

COPY . .
RUN dotnet publish SpeedRunners.Scheduler.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/runtime:3.1 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SpeedRunners.Scheduler.dll"]
