FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Kopiera solution-filen
COPY ["MerchStore.sln", "./"]

# Kopiera alla projektmappar
COPY ["src/MerchStore.Domain/", "./src/MerchStore.Domain/"]
COPY ["src/MerchStore.Application/", "./src/MerchStore.Application/"]
COPY ["src/MerchStore.Infrastructure/", "./src/MerchStore.Infrastructure/"]
COPY ["src/MerchStore.WebUI/", "./src/MerchStore.WebUI/"]
COPY ["infra/", ".infra/"]

# Kopiera eventuella testprojekt om de behövs för bygget
COPY ["tests/", "./tests/"]

# Återställ alla paket
RUN dotnet restore

# Bygg projektet (anpassar sökvägen till WebUI-projektet)
WORKDIR "/src/src/MerchStore.WebUI"
RUN dotnet build "MerchStore.WebUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MerchStore.WebUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Lägg till denna rad i din Dockerfile, innan ENTRYPOINT
ENV ASPNETCORE_URLS=http://+:80
ENV ConnectionStrings__DefaultConnection="Server=tcp:merchstore.database.windows.net,1433;Initial Catalog=merchstoredb;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";"
ENTRYPOINT ["dotnet", "MerchStore.WebUI.dll"]
