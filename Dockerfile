# Dockerfile i root
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/MerchStore.WebUI/MerchStore.WebUI.csproj", "src/MerchStore.WebUI/"]
COPY ["src/MerchStore.Infrastructure/MerchStore.Infrastructure.csproj", "src/MerchStore.Infrastructure/"]
COPY ["src/MerchStore.Application/MerchStore.Application.csproj", "src/MerchStore.Application/"]
COPY ["src/MerchStore.Domain/MerchStore.Domain.csproj", "src/MerchStore.Domain/"]
RUN dotnet restore "src/MerchStore.WebUI/MerchStore.WebUI.csproj"
COPY . .
WORKDIR "/src/src/MerchStore.WebUI"
RUN dotnet build "MerchStore.WebUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MerchStore.WebUI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MerchStore.WebUI.dll"]