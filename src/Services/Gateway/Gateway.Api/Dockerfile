# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY src/Services/Gateway/Gateway.Api/Gateway.Api.csproj src/Services/Gateway/Gateway.Api/

RUN dotnet restore src/Services/Gateway/Gateway.Api/Gateway.Api.csproj

COPY . .

RUN dotnet publish src/Services/Gateway/Gateway.Api/Gateway.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

RUN adduser --disabled-password appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

EXPOSE 8000
ENV ASPNETCORE_URLS=http://+:8000

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
