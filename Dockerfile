FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY NuGet.config .
COPY Sinance.Web/*.csproj ./Sinance.Web/
COPY Sinance.Business/*.csproj ./Sinance.Business/
COPY Sinance.Common/*.csproj ./Sinance.Common/
COPY Sinance.Communication/*.csproj ./Sinance.Communication/
COPY Sinance.Storage/*.csproj ./Sinance.Storage/
RUN dotnet restore

# copy everything else and build app
COPY . .
WORKDIR /app/Sinance.Web
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-bionic AS runtime

WORKDIR /app
COPY --from=build /app/Sinance.Web/out .

ENTRYPOINT ["dotnet", "Sinance.Web.dll"]