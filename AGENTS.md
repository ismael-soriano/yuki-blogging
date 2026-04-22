# AI Agents Guidelines

This repository is designed to support AI-assisted development.  
All agents must follow these rules when modifying or generating code.

## Architecture Principles
- Follow Hexagonal Architecture (Ports & Adapters)
- Domain layer must remain independent of infrastructure
- Application layer handles use cases (CQRS style)
- Infrastructure implements external concerns (DB, API, serialization)
- Keep service boundaries explicit: `src/Services/Authors/*` and `src/Services/Posts/*` are separate microservices
- In Posts, preserve write/read separation: `Posts.Domain/Aggregates/PostAggregate.cs` emits events, `IPostEventStore` persists them, and `IPostReadRepository` serves queries

## Coding Rules
- Do not introduce logic in controllers
- Keep domain entities free of framework dependencies
- Prefer composition over inheritance
- Avoid tight coupling between layers
- Keep domain validation in domain types (e.g., `Authors.Domain/Entities/Author.cs`, `Posts.Domain/Aggregates/PostAggregate.cs`)
- Keep cross-service HTTP calls in infrastructure adapters only (e.g., `Posts.Infrastructure/External/AuthorsServiceClient.cs`)

## API Design
- RESTful conventions must be followed
- Support future content negotiation (JSON/XML)
- Do not break existing contracts
- Preserve existing routes and query contract: `GET /authors/{id}`, `POST /post`, `GET /post/{id}`, `GET /post/{id}?includeAuthor=true`
- Keep content negotiation settings in API startup (`RespectBrowserAcceptHeader`, `ReturnHttpNotAcceptable`) and current JSON output contract

## Testing
- Maintain at least 90% coverage
- Prefer unit tests for domain logic
- Add integration tests for endpoints
- Keep test projects split by purpose:
  - Unit tests: `tests/*/*.UnitTests`
  - API integration tests: `tests/*/*.Api.IntegrationTests`
- Current expected test project layout:
  - `tests/Authors/Authors.Api.UnitTests`
  - `tests/Authors/Authors.Api.IntegrationTests`
  - `tests/Posts/Posts.UnitTests`
  - `tests/Posts/Posts.Api.IntegrationTests`
- Unit test naming convention is mandatory:
  - File name: `<ClassUnderTest>UnitTests.cs`
  - Class name: `<ClassUnderTest>UnitTests`
  - Example: `PostAggregateUnitTests.cs` / `PostAggregateUnitTests`
- Integration test naming convention:
  - File name: `<ControllerOrFeature>IntegrationTests.cs`
  - Class name: `<ControllerOrFeature>IntegrationTests`
- Run commands from solution root:
  - Full suite with coverage: `dotnet test Yuki.Blogging.sln --collect:"XPlat Code Coverage"`
  - Unit tests only: `dotnet test Yuki.Blogging.sln --filter "UnitTests"`
  - Integration tests only: `dotnet test Yuki.Blogging.sln --filter "IntegrationTests"`
- Before committing, ensure the solution compiles and relevant tests pass for the change set

## When Adding Features
1. Define domain model first
2. Add command/query
3. Implement handler
4. Add repository interface if needed
5. Implement infrastructure
6. Add tests
7. For Posts write-side changes, update both event persistence (`IPostEventStore`) and read projection (`IPostReadRepository`)
8. Deliver changes in small, atomic increments so each commit remains buildable and test-validated

## Forbidden Actions
- Do not bypass domain layer
- Do not add business logic in infrastructure
- Do not remove tests without replacement
- Do not call Authors API directly from controllers/handlers; depend on `IAuthorDirectory` and keep HTTP in infrastructure

## Preferred Patterns
- CQRS (lightweight)
- Dependency Injection
- Clean separation of concerns
- Register layer services via DI extension methods (`AddAuthorsApplication`, `AddAuthorsInfrastructure`, `AddPostsApplication`, `AddPostsInfrastructure`)
- Use `WebApplicationFactory<Program>` and `ConfigureTestServices` to isolate API integration tests from external dependencies

## Notes
- This project is intentionally over-engineered for demonstration purposes.
- Focus on clarity, maintainability, and extensibility.
- Local defaults: Authors API on `http://localhost:8081`, Posts API on `http://localhost:8080`, and Posts->Authors base URL via `Services__Authors__BaseUrl`
