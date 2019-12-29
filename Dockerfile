FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY NuGet.config .
COPY Sinance.Web/Sinance.Web.csproj ./Sinance.Web/
COPY Sinance.Business/Sinance.Business.csproj ./Sinance.Business/
COPY Sinance.Common/Sinance.Common.csproj ./Sinance.Common/
COPY Sinance.Communication/Sinance.Communication.csproj ./Sinance.Communication/
COPY Sinance.Storage/Sinance.Storage.csproj ./Sinance.Storage/

RUN dotnet restore Sinance.Web/Sinance.Web.csproj &&\
    dotnet restore Sinance.Business/Sinance.Business.csproj &&\
    dotnet restore Sinance.Common/Sinance.Common.csproj &&\
    dotnet restore Sinance.Communication/Sinance.Communication.csproj &&\
    dotnet restore Sinance.Storage/Sinance.Storage.csproj

# copy everything else and build app
COPY . .

RUN dotnet publish Sinance.Web/Sinance.Web.csproj -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-bionic AS runtime

WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Sinance.Web.dll"]