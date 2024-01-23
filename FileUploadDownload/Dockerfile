#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mkodockx/docker-clamav:alpine as clamav
EXPOSE 3310

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FileUploadDownload/FileUploadDownload.csproj", "FileUploadDownload/"]
RUN dotnet restore "FileUploadDownload/FileUploadDownload.csproj"
COPY . .
WORKDIR "/src/FileUploadDownload"
RUN dotnet build "FileUploadDownload.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileUploadDownload.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileUploadDownload.dll"]