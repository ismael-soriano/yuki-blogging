# Yuki Blogging

Backend technical test implementation for a software company assessment.

This solution exposes a small blogging platform through a microservice-based backend composed of:

- `Authors API`: manages authors in SQL Server.
- `Posts API`: manages blog posts in MongoDB and can enrich post responses with author data.
- `Gateway API`: Ocelot-based reverse proxy that provides a single entry point and aggregated Swagger UI.

The codebase follows a layered, ports-and-adapters approach with clear separation between domain, application, API, and infrastructure concerns.

## Solution Overview

### Services

#### Authors

- Technology: ASP.NET Core 8
- Storage: SQL Server
- Responsibilities:
	- Create, list, update, and delete authors
	- Expose JSON and XML responses

Main endpoints:

- `GET /authors`
- `GET /authors/{id}`
- `POST /authors`
- `PUT /authors/{id}`
- `DELETE /authors/{id}`

#### Posts

- Technology: ASP.NET Core 8
- Storage: MongoDB
- Responsibilities:
	- Create, list, update, and soft-delete posts
	- Validate that the referenced author exists
	- Optionally include author information in query responses

Main endpoints:

- `GET /post?page=1&pageSize=10&includeAuthor=false`
- `GET /post/{id}`
- `GET /post/{id}?includeAuthor=true`
- `POST /post`
- `PUT /post/{id}`
- `DELETE /post/{id}`

#### Gateway

- Technology: Ocelot
- Responsibilities:
	- Route requests to the Authors and Posts services
	- Aggregate Swagger documents behind a single UI

Default gateway routes:

- `/authors/*`
- `/post/*`

## Architecture

The repository is intentionally structured as a technical exercise, with explicit service boundaries and layered responsibilities.

- `src/Services/Authors/*`: Authors microservice
- `src/Services/Posts/*`: Posts microservice
- `src/Services/Gateway/*`: API gateway
- `tests/*`: unit and integration tests

Within each service:

- `*.Domain`: business rules and core domain model
- `*.Application`: use cases, commands, queries, and ports
- `*.Infrastructure`: persistence and external integrations
- `*.Api`: HTTP endpoints and startup configuration

The Posts service also uses a read/write split, with an event-driven aggregate on the write side and a read repository for queries.

## What You Need To Run Locally

### Required tools

- .NET SDK 8.0
- Docker Desktop or Docker Engine with Docker Compose support

### Recommended tools

- Visual Studio 2026 or Rider or vscode
- A REST client such as Postman, Insomnia or curl

### Local infrastructure used by the solution

- SQL Server 2022 container for Authors
- MongoDB 7 container for Posts

## Quick Start With Docker

This is the simplest way to run the full backend locally.

From the solution root:

```bash
docker compose up -d
```

This starts:

- SQL Server for Authors
- MongoDB for Posts
- Authors API
- Posts API
- API Gateway

### Exposed local URLs

- Gateway: `http://localhost:5000`
- Gateway Swagger: `http://localhost:5000/swagger`
- Authors API: `http://localhost:5001/swagger`
- Posts API: `http://localhost:5002/swagger`
- SQL Server: `localhost:1433`
- MongoDB: `localhost:27017`

### Seeded and default credentials

The docker setup uses these local development credentials:

- SQL Server user: `sa`
- SQL Server password: `YourPassword123!`
- MongoDB root user: `root`
- MongoDB root password: `YourPassword123!`

## Debugging with Docker Compose

You can execute the docker compose file within Rider or Visual Studio 2026 and it will attach to the Authors, Posts and Gateway services.

## Running Without Docker Compose

If you want to run the APIs directly from the CLI or IDE, you still need SQL Server and MongoDB running locally.

### 1. Start the databases

You can use the containers only for infrastructure:

```bash
docker compose up -d posts-db authors-db authors-db-init
```

### 2. Run Authors API

Authors uses this connection string by default:

```text
Server=localhost,1433;Database=authors;User Id=sa;Password=YourPassword123!;TrustServerCertificate=True;Encrypt=False;
```

Run it with:

```bash
dotnet run --project src/Services/Authors/Authors.Api
```

### 3. Run Posts API

Posts expects:

- MongoDB at `mongodb://localhost:27017`
- Database name `posts`
- Authors service base URL in `Services:Authors:BaseUrl`

If the Authors API is not running on `http://localhost:8081`, override it with an environment variable before starting Posts.

PowerShell:

```powershell
$env:Services__Authors__BaseUrl="http://localhost:5001"
dotnet run --project src/Services/Posts/Posts.Api
```

Or, if Authors is running on another port, replace `5001` with the correct value.

### 4. Run Gateway API

The gateway configuration in `src/Services/Gateway/Gateway.Api/appsettings.json` is container-oriented, so for local non-container execution you should either:

- run everything through Docker Compose, or
- adjust gateway downstream hosts to match your local process URLs

If you want to run the gateway directly:

```bash
dotnet run --project src/Services/Gateway/Gateway.Api
```

## Testing

Run tests from the solution root.

### Full test suite with coverage

```bash
dotnet test Yuki.Blogging.sln --collect:"XPlat Code Coverage"
```

### Unit tests only

```bash
dotnet test Yuki.Blogging.sln --filter "UnitTests"
```

### Integration tests only

```bash
dotnet test Yuki.Blogging.sln --filter "IntegrationTests"
```

## Example Requests

### Create an author

```http
POST /authors
Content-Type: application/json

{
	"name": "Ada",
	"surname": "Lovelace"
}
```

### Create a post

```http
POST /post
Content-Type: application/json

{
	"authorId": "9f9df8ca-4314-4d0d-a629-fcb0cead5dae",
	"title": "My first post",
	"description": "Short summary",
	"content": "Post body"
}
```

### Query a post with author details

```http
GET /post/{id}?includeAuthor=true
Accept: application/json
```

## Notes For Reviewers

- The solution is designed as a backend technical test rather than a production-complete platform.
- The main focus is service separation, API design, validation, persistence integration, and automated tests.
- Both Authors and Posts APIs support content negotiation for JSON and XML.
- The gateway provides a single entry point for manual evaluation.

## Repository Structure

```text
.
|-- src/
|   |-- Services/
|   |   |-- Authors/
|   |   |-- Posts/
|   |   `-- Gateway/
|-- tests/
|   |-- Authors/
|   `-- Posts/
|-- docker-compose.yml
`-- Yuki.Blogging.sln
```
