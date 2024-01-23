#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
# WORKDIR /src
# COPY ["SpeedRunners/SpeedRunners.csproj", "SpeedRunners/"]
# COPY ["SpeedRunners.BLL/SpeedRunners.BLL.csproj", "SpeedRunners.BLL/"]
# COPY ["SpeedRunners.DAL/SpeedRunners.DAL.csproj", "SpeedRunners.DAL/"]
# COPY ["SpeedRunners.Utils/SpeedRunners.Utils.csproj", "SpeedRunners.Utils/"]
# COPY ["SpeedRunners.Model/SpeedRunners.Model.csproj", "SpeedRunners.Model/"]
# RUN dotnet restore "SpeedRunners/SpeedRunners.csproj"
# COPY . .
# WORKDIR "/src/SpeedRunners"
# RUN dotnet build "SpeedRunners.csproj" -c debug -o /app/build

# FROM build AS publish
# RUN dotnet publish "SpeedRunners.csproj" -c debug -o /app/publish /p:UseAppHost=false
#WORKDIR /app
#COPY --from=publish /app/publish .

COPY ./publish .

ENTRYPOINT ["dotnet", "SpeedRunners.dll"]