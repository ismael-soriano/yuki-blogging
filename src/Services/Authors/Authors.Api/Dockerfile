# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY src/Services/Authors/Authors.Api/Authors.Api.csproj src/Services/Authors/Authors.Api/
COPY src/Services/Authors/Authors.Application/Authors.Application.csproj src/Services/Authors/Authors.Application/
COPY src/Services/Authors/Authors.Domain/Authors.Domain.csproj src/Services/Authors/Authors.Domain/
COPY src/Services/Authors/Authors.Infrastructure/Authors.Infrastructure.csproj src/Services/Authors/Authors.Infrastructure/

RUN dotnet restore src/Services/Authors/Authors.Api/Authors.Api.csproj

COPY . .

RUN dotnet publish src/Services/Authors/Authors.Api/Authors.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

RUN adduser --disabled-password appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Authors.Api.dll"]