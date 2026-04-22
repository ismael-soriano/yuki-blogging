FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/Gateway/Gateway.Api/Gateway.Api.csproj", "src/Services/Gateway/Gateway.Api/"]

RUN dotnet restore "src/Services/Gateway/Gateway.Api/Gateway.Api.csproj"

COPY . .

RUN dotnet build "src/Services/Gateway/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Services/Gateway/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8000

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]

