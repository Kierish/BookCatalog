FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/BookCatalog.Api/BookCatalog.Api.csproj", "src/BookCatalog.Api/"]
COPY ["src/BookCatalog.Domain/BookCatalog.Domain.csproj", "src/BookCatalog.Domain/"]
COPY ["src/BookCatalog.Infrastructure/BookCatalog.Infrastructure.csproj", "src/BookCatalog.Infrastructure/"]

RUN dotnet restore "src/BookCatalog.Api/BookCatalog.Api.csproj"

COPY . .

RUN dotnet publish "src/BookCatalog.Api/BookCatalog.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false


FROM build AS migration

RUN dotnet tool install \
    --tool-path /dotnet-tools \
    dotnet-ef \
    --version 10.0.11

ENTRYPOINT ["/dotnet-tools/dotnet-ef", "database", "update", "--project", "src/BookCatalog.Infrastructure/BookCatalog.Infrastructure.csproj", "--startup-project", "src/BookCatalog.Api/BookCatalog.Api.csproj", "--configuration", "Release", "--no-build"]


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "BookCatalog.Api.dll"]
