# API Gateway - Ocelot-based Reverse Proxy

The API Gateway is a centralized entry point that routes all requests to the underlying microservices (Authors and Posts) using [Ocelot](https://ocelot.readthedocs.io/). Swagger specifications from all microservices are aggregated automatically via [MMLib.SwaggerForOcelot](https://github.com/Burgyn/MMLib.SwaggerForOcelot).

## Features

- **Unified Routing**: Routes all `/authors/*` and `/post/*` requests to the respective microservices
- **Swagger Aggregation**: Swagger specs from all services are merged declaratively via `appsettings.json` using `SwaggerEndPoints` and `SwaggerKey` route annotations
- **Unified Swagger UI**: Provides a single Swagger UI at `/swagger` with all microservices documented
- **Configuration-driven**: No code changes are needed to add new routes or services
- **Docker-ready**: Includes a Dockerfile for containerized deployment

## Project Structure

```
src/Services/Gateway/Gateway.Api/
├── Program.cs           # ASP.NET Core startup with Ocelot and SwaggerForOcelot
├── appsettings.json     # Ocelot routes + Swagger endpoint configuration
├── Dockerfile           # Multi-stage container build
└── Gateway.Api.csproj   # Project file with Ocelot and Swagger dependencies
```

## Configuration

All routing and Swagger aggregation is configured through `appsettings.json`:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/authors/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "authors-api", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/authors/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete", "Patch" ],
      "SwaggerKey": "authors"
    },
    {
      "DownstreamPathTemplate": "/post/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "posts-api", "Port": 8080 }
      ],
      "UpstreamPathTemplate": "/post/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete", "Patch" ],
      "SwaggerKey": "posts"
    }
  ],
  "SwaggerEndPoints": [
    {
      "Key": "authors",
      "Config": [
        {
          "Name": "Authors API",
          "Version": "v1",
          "Url": "http://authors-api:8080/swagger/v1/swagger.json"
        }
      ]
    },
    {
      "Key": "posts",
      "Config": [
        {
          "Name": "Posts API",
          "Version": "v1",
          "Url": "http://posts-api:8080/swagger/v1/swagger.json"
        }
      ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "http://api-gateway:8080"
  }
}
```

### Configuration Structure

- **Routes**: Array of Ocelot routes with:
  - `DownstreamPathTemplate` / `DownstreamHostAndPorts`: Target service address and path
  - `UpstreamPathTemplate`: Incoming path pattern matched on the gateway
  - `UpstreamHttpMethod`: HTTP methods accepted
  - `SwaggerKey`: Links this route to a `SwaggerEndPoints` entry for UI aggregation
- **SwaggerEndPoints**: Array of upstream Swagger specs with:
  - `Key`: Must match the `SwaggerKey` on the relevant route(s)
  - `Config[].Url`: URL to the upstream OpenAPI JSON endpoint
  - `Config[].Name` / `Config[].Version`: Display metadata in the Swagger UI
- **GlobalConfiguration.BaseUrl**: Public base URL used by Ocelot when constructing responses

## Running the Gateway

### Local Development with Docker Compose

```bash
# Start all the environment with the docker-compose project for debugging or execute
docker-compose up -d
```

Then access:
- **Composed Swagger UI**: `http://localhost:8000/swagger`
- **Routed Authors API**: `http://localhost:8000/authors/{id}`
- **Routed Posts API**: `http://localhost:8000/post`

## Swagger Aggregation

`MMLib.SwaggerForOcelot` is driven entirely by the `SwaggerEndPoints` configuration and `SwaggerKey` annotations on routes:

1. Each route carries a `SwaggerKey` that maps it to a `SwaggerEndPoints` entry.
2. At startup, `AddSwaggerForOcelot` registers the aggregation middleware.
3. `UseSwaggerForOcelotUI` exposes the combined Swagger UI and serves individual upstream specs via `/swagger/docs`.
4. The Swagger UI dropdown lets users switch between `Authors API` and `Posts API` documentation.

## Adding a New Microservice

Update `appsettings.json` only — no code changes required.

### 1. Add a Route:
```json
{
  "DownstreamPathTemplate": "/myservice/{everything}",
  "DownstreamScheme": "http",
  "DownstreamHostAndPorts": [
    { "Host": "myservice-api", "Port": 8080 }
  ],
  "UpstreamPathTemplate": "/myservice/{everything}",
  "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete", "Patch" ],
  "SwaggerKey": "myservice"
}
```

### 2. Add a SwaggerEndPoint:
```json
{
  "Key": "myservice",
  "Config": [
    {
      "Name": "My Service API",
      "Version": "v1",
      "Url": "http://myservice-api:8080/swagger/v1/swagger.json"
    }
  ]
}
```

No recompilation needed — Ocelot and SwaggerForOcelot read configuration at startup.

## Port Mapping

| Service | Docker Internal Port | Host Port | Via Gateway |
|---------|---------------------|-----------|-------------|
| Authors | 8080 | 5001      | `localhost:8000/authors` |
| Posts | 8080 | 5002      | `localhost:8000/post` |
| Gateway | 8080 | 5000      | — |

## Dependencies

- **Ocelot 24.0.0**: API gateway routing and load balancing
- **MMLib.SwaggerForOcelot 8.3.0**: Swagger UI aggregation for Ocelot routes
- **Swashbuckle.AspNetCore 6.6.2**: Swagger UI and documentation generation

## Environment Variables (Docker)

Downstream host/port are configured in `appsettings.json`. To override at runtime, use standard ASP.NET Core flat-key notation:

```bash
export Routes__0__DownstreamHostAndPorts__0__Host=my-authors-host
export Routes__1__DownstreamHostAndPorts__0__Host=my-posts-host
```

Or in `docker-compose.yml`:
```yaml
environment:
  Routes__0__DownstreamHostAndPorts__0__Host: authors-api
  Routes__1__DownstreamHostAndPorts__0__Host: posts-api
```
