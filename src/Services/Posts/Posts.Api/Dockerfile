# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY src/Services/Posts/Posts.Api/Posts.Api.csproj src/Services/Posts/Posts.Api/
COPY src/Services/Posts/Posts.Application/Posts.Application.csproj src/Services/Posts/Posts.Application/
COPY src/Services/Posts/Posts.Domain/Posts.Domain.csproj src/Services/Posts/Posts.Domain/
COPY src/Services/Posts/Posts.Infrastructure/Posts.Infrastructure.csproj src/Services/Posts/Posts.Infrastructure/

RUN dotnet restore src/Services/Posts/Posts.Api/Posts.Api.csproj

COPY . .

RUN dotnet publish src/Services/Posts/Posts.Api/Posts.Api.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

RUN adduser --disabled-password appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Posts.Api.dll"]
