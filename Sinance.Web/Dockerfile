#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base

RUN apk add tzdata
ENV TZ "Europe/Amsterdam"

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN apk add --no-cache icu-libs
ENV LC_ALL=nl_NL.UTF-8
ENV LANG=nl_NL.UTF-8

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["NuGet.config", "."]
COPY ["Sinance.Web/Sinance.Web.csproj", "Sinance.Web/"]
COPY ["Sinance.Business/Sinance.Business.csproj", "Sinance.Business/"]
COPY ["Sinance.Storage/Sinance.Storage.csproj", "Sinance.Storage/"]
COPY ["Sinance.Common/Sinance.Common.csproj", "Sinance.Common/"]
COPY ["Sinance.Communication/Sinance.Communication.csproj", "Sinance.Communication/"]
RUN dotnet restore "Sinance.Web/Sinance.Web.csproj"
COPY . .
WORKDIR "/src/Sinance.Web"
RUN dotnet build "Sinance.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sinance.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sinance.Web.dll"]