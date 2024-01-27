#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

#FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["SpeedRunners.Scheduler.csproj", "."]
#RUN dotnet restore "./././SpeedRunners.Scheduler.csproj"
#COPY . .
#WORKDIR "/src/."
#RUN dotnet build "./SpeedRunners.Scheduler.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./SpeedRunners.Scheduler.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
COPY . .
ENTRYPOINT ["dotnet", "SpeedRunners.Scheduler.dll"]