# Design Note — Book Catalog Platform (Weeks 1–4)

A record of the platform's architectural decisions, trade-offs, and learnings across the four weeks.

---

## 1. What the Platform Does

Book Catalog Platform is a REST API built with .NET 10 for managing a book catalog and borrowing operations.

The platform allows clients to create, retrieve, update, delete, search, and paginate books. It also supports borrowing and returning books, viewing loan history, and preventing multiple active loans for the same book.

The application uses PostgreSQL with EF Core for persistent storage, Docker Compose for local deployment, and provides validation, consistent error responses, logging, and health checks. Unit and integration tests cover business logic and the main API flows.

---

## 2. Week 1 — Initial Implementation

### Architecture & Solution Structure

The initial solution used a **Feature-First (Vertical Slice) Architecture** within a single project:

```text
src/BookCatalog/
├── Domain/
│   └── Book.cs                    # Entity with Guid V7 identifiers
├── Features/Books/
│   ├── CreateBook/                # Request, Validator, Handler
│   ├── DeleteBook/                # Handler
│   ├── GetBookById/               # Handler
│   ├── GetBooksList/              # Handler
│   ├── UpdateBook/                # Request, Validator, Handler
│   ├── BookResponse.cs            # Shared response contract
│   └── BooksController.cs         # REST routing and status code mapping
└── Infrastructure/
    ├── Filters/
    │   └── ValidationFilter.cs    # FluentValidation action filter
    └── Storage/
        └── InMemoryBookStore.cs   # In-memory storage
```

The API supported CRUD operations with request validation, Swagger documentation, logging, and appropriate HTTP status codes. Data was stored in memory using `InMemoryBookStore` and was lost when the application restarted.

### Key Decisions and Rationale

| Decision | Why |
| :--- | :--- |
| Organized the project using Vertical Slices. | Groups code by feature rather than technical responsibility, making individual operations easier to locate and modify. |
| Used `ConcurrentDictionary<Guid, Book>` registered as a `Singleton`. | Supports thread-safe dictionary operations for concurrent HTTP requests and fast lookups. The singleton lifetime preserves the data for the application's lifetime. |
| Generated IDs using `Guid.CreateVersion7()`. | Generates time-ordered identifiers with a very low collision probability. Their sequential characteristics can improve database index locality compared with random UUIDv4 identifiers. |
| Implemented FluentValidation through a custom `ValidationFilter`. | Keeps validation rules separate from request models and provides flexibility beyond DataAnnotations. The filter returns consistent validation errors using `ProblemDetails`. |
| Used `Task` / `Task<T>` return types with `Task.CompletedTask` and `Task.FromResult` in the in-memory store. | Established asynchronous contracts for future EF Core integration while avoiding unnecessary async state machines for synchronous in-memory operations. |

### What I Found Hard

- **Choosing the Architecture:** Finding the right balance between avoiding overengineering for Week 1 and establishing a foundation that could evolve as the project grew.
- **Sticking to YAGNI:** Resisting the temptation to introduce unnecessary features and abstractions, such as additional audit fields, soft deletion, or premature layering.

### Improvements Identified at the End of Week 1

1. **Automated Integration Tests:** Add HTTP-level tests using `WebApplicationFactory` to verify routing, status codes, and validation behavior. *Implemented in Week 4.*
2. **Querying Enhancements:** Introduce pagination with total count and filtering for `GET /api/books`. *Implemented in Week 2.*

---

## 3. Week 2 — Architecture & Testing

### Layer Separation and Rationale

I restructured the single-project solution into three projects:

- **Domain:** Contains entities, shared models, and repository interfaces. It has no dependencies on other solution projects.
- **Infrastructure:** Contains the in-memory repository implementation and handles data storage.
- **Api:** Contains controllers, feature handlers, validators, mappings, and application configuration.

I kept feature handlers in `Api` rather than introducing a separate `Application` project. At this scale, another project would add complexity without providing a clear benefit. Controllers and handlers still have separate responsibilities.

This structure separates storage concerns from business logic and allows the persistence implementation to change without requiring changes to the handlers, provided the repository contract remains compatible.

### Data Access Abstraction

I introduced `IBookRepository` to replace direct dependencies on `InMemoryBookStore`.

The interface defines operations for retrieving, creating, updating, and deleting books, including paginated and filtered queries.

It hides the storage implementation from handlers, allowing them to work with an abstraction rather than knowing whether the data is stored in memory or in a database.

The repository implementation is selected through dependency injection. This prepared the project for replacing in-memory storage with EF Core in Week 3.

### Testing Strategy

I used **xUnit, NSubstitute, and Shouldly** to write unit tests without external dependencies.

- **Handlers:** Tested operation results, repository interactions, and missing-book scenarios.
- **Mappings and Pagination:** Tested field conversions, page calculations, and metadata.
- **Validators:** Tested valid inputs, invalid values, and boundary conditions.
- **Exception Handling:** Verified consistent error responses without exposing internal exception details.

I prioritized business behavior and error paths rather than aiming for 100% code coverage.

I did not focus on logging assertions, every possible exception-propagation scenario, or testing the HTTP pipeline and database interactions. Those integration scenarios were left for Week 4.

Repository filtering and pagination implementations were not tested directly; handler tests verified that the correct parameters were forwarded.

### Refactoring Challenges and Lessons

The main refactoring challenge was replacing the direct dependency on `InMemoryBookStore` across the existing handlers.

Although the restructuring was mostly mechanical, every handler constructor had to be updated to depend on `IBookRepository`.

This revealed a weakness in the original design: handlers were coupled to a concrete storage implementation, making it harder to replace the persistence mechanism.

Introducing the repository interface removed that dependency and made the storage implementation replaceable without directly affecting the business logic.

---

## 4. Week 3 — Persistence & Docker

### Data Model

- **Author → Books:** One author can have many books. Authors are stored separately to avoid duplicating information.
- **Book → Loans:** One book can have multiple loans over time. `ReturnedAt = null` indicates an active loan, while previous loans remain in the history.
- **User → Loans:** One user can borrow multiple books. Each `Loan` stores `BorrowedAt` and `ReturnedAt`.
- **Loan Identity:** Each loan has its own primary key because the same user can borrow the same book multiple times.
- **Active Loan Count:** `User.ActiveLoanCount` stores the number of active loans and is updated in the same transaction as borrowing or returning a book.

Database constraints enforce data integrity, including foreign keys, valid publication years, non-negative loan counts, and valid return dates. A partial unique index prevents multiple active loans for the same book.

### Database Choice: PostgreSQL

I chose PostgreSQL for four reasons:

1. **Docker:** The `postgres:17-alpine` image is relatively small and easy to run locally.
2. **UUIDv7:** Time-ordered UUIDs can improve index locality compared with random UUIDv4 values.
3. **Partial Unique Indexes:** A unique index on `BookId` where `ReturnedAt IS NULL` prevents two active loans for the same book.
4. **Open Source:** PostgreSQL is free and has no licensing restrictions.

### Impact of Replacing In-Memory Storage

The transition affected entities, repositories, DTOs, mappings, validators, handlers, controllers, and unit tests.

However, most changes came from expanding the domain with authors, users, and loans, rather than replacing the storage itself.

The `IBookRepository` abstraction helped keep EF Core inside `Infrastructure`, while handlers continued depending on repository interfaces.

I also introduced the **Result pattern** to handle expected failures, such as missing entities or unavailable books, without using exceptions.

**Lesson learned:** The repository abstraction made replacing storage easier, but expanding the domain still required changes across multiple layers.

### Expected Performance Bottleneck

The first expected bottleneck is `GET /api/books`.

It executes count and page queries, uses `ILIKE '%value%'` for searching, and relies on offset pagination.

Searching with a leading wildcard can scan many rows, while large offsets become slower as the database skips earlier results.

**Possible improvements:** PostgreSQL trigram indexes, keyset pagination, and DTO projection with `Select`.

### Dockerfile Structure

| Instruction | Purpose |
| :--- | :--- |
| `FROM .../sdk:10.0 AS build` | Uses the .NET SDK to build the application. |
| `WORKDIR /src` | Sets the working directory. |
| `COPY` project files + `dotnet restore` | Restores dependencies using Docker layer caching. |
| `COPY . .` | Copies the remaining source code. |
| `dotnet publish --no-restore` | Builds and publishes the API in Release mode. |
| `FROM build AS migration` | Creates a separate stage for EF Core migrations. |
| `dotnet tool install ... dotnet-ef` | Installs the EF Core CLI. |
| Migration `ENTRYPOINT` | Applies migrations before the API starts. |
| `FROM .../aspnet:10.0 AS final` | Uses a smaller runtime-only image. |
| `ENV ASPNETCORE_HTTP_PORTS=8080` + `EXPOSE 8080` | Configures the API port. |
| `COPY --from=build /app/publish .` | Copies the published files into the runtime image. |
| `USER $APP_UID` | Runs the API without root privileges. |
| API `ENTRYPOINT` | Starts `BookCatalog.Api.dll`. |

---
 
## 5. Week 4 — Final Reflection

### Week 4 Overview

In the final week, I focused on making the API more reliable, testable, and easier to maintain. I added integration tests against a real PostgreSQL database, health checks, structured JSON logging with Serilog, startup configuration validation, bounded database retries, and graceful shutdown.

I also refactored existing code, optimized database operations, introduced `TimeProvider` for date-dependent logic, and updated the README.

### Final Architecture & Solution Structure

```text
src/
├── BookCatalog.Domain/
│   ├── Entities/                    # Author, Book, User, and Loan entities
│   ├── Common/                      # Pagination, queries, and Result models
│   └── Interfaces/                  # Repository contracts
├── BookCatalog.Infrastructure/
│   ├── Persistence/
│   │   ├── Configurations/          # EF Core entity mappings and constraints
│   │   ├── Migrations/              # PostgreSQL schema migrations
│   │   └── BookCatalogDbContext.cs  # EF Core database context
│   └── Repositories/                # PostgreSQL repository implementations
└── BookCatalog.Api/
    ├── Configuration/               # Database options and startup validation
    ├── Controllers/                 # Shared API controller behavior
    ├── Exceptions/                  # Global exception handling
    ├── Features/
    │   ├── Books/
    │   │   ├── CreateBook/          # Request, Validator, Handler
    │   │   ├── DeleteBook/          # Handler
    │   │   ├── GetBookById/         # Handler
    │   │   ├── GetBooks/            # Request, Validator, Handler
    │   │   ├── UpdateBook/          # Request, Validator, Handler
    │   │   ├── Shared/              # Response, mappings, ISBN validation
    │   │   └── BooksController.cs   # Book REST endpoints
    │   └── Loans/
    │       ├── BorrowBook/           # Request, Validator, Handler
    │       ├── GetLoanHistory/       # Request, Validator, Handler, Response
    │       ├── ReturnBook/           # Handler
    │       ├── Shared/               # Loan responses and mappings
    │       └── LoansController.cs    # Lending REST endpoints
    ├── Filters/                      # FluentValidation action filter
    └── Program.cs                    # Dependency registration and HTTP pipeline
tests/
├── BookCatalog.UnitTests/            # Isolated business-logic tests
└── BookCatalog.IntegrationTests/
    ├── Features/                     # HTTP tests for core API flows
    └── Infrastructure/               # TestServer, Testcontainers, and Respawn setup
Dockerfile                             # Build, migration, and runtime stages
docker-compose.yml                     # PostgreSQL, migrations, and API orchestration
```

### Key Decisions and Rationale

These are the five most important technical decisions made throughout the month.

| Decision | Rationale and Trade-off |
| :--- | :--- |
| **Feature-Based Layered Architecture** | Combined feature-based organization with separate `Domain`, `Infrastructure`, and `Api` projects. This keeps related functionality together while separating responsibilities. Handlers remain in `Api` to avoid adding an unnecessary `Application` project, at the cost of keeping application logic and HTTP concerns in the same assembly. |
| **Database Choice: PostgreSQL** | Chose PostgreSQL because it is free, works well with Docker, supports UUIDv7 identifiers, and provides partial unique indexes. The trade-off is relying on some PostgreSQL-specific features. |
| **Partial Unique Index for Lending** | Used a partial unique index on `BookId` where `ReturnedAt IS NULL`. This prevents multiple active loans for the same book at the database level while preserving borrowing history. |
| **Real Database Integration Testing** | Used `WebApplicationFactory` with `TestServer` to test the API without running it in Docker or using real Kestrel network connections. PostgreSQL runs in a shared Testcontainers container, and Respawn resets the database before each test. Tests run sequentially to avoid conflicts. This reduces setup overhead but does not verify real network behavior. |
| **Result Pattern vs Exceptions** | Used the Result pattern for expected failures such as `NotFound` and `Conflict`, while reserving exceptions for unexpected failures. This makes business outcomes explicit but requires additional result types and handling logic. |

### What I Would Do Differently

- **Repository Abstraction:** I would introduce `IBookRepository` in Week 1 instead of making handlers depend directly on `InMemoryBookStore`. This would have avoided changing handler dependencies during Week 2 and made replacing the storage implementation easier.
- **Soft Deletion:** When loan history was introduced in Week 3, I would choose soft deletion instead of physical deletion. This would allow books to be removed from the catalog while preserving their borrowing history.
- **TimeProvider:** I would introduce `TimeProvider` when implementing the first date-dependent validation rule. This would make the dependency on the current year explicit and allow tests to use a controlled clock instead of relying on the system time.

### Known Limitations and Performance Bottlenecks

- **Concurrency:** Simultaneous borrowing or returning operations may cause incorrect `ActiveLoanCount` values. Transactions ensure atomicity but do not prevent every race condition.
- **Book Deletion Limitations:** Books can only be permanently deleted if they have no borrowing history. Otherwise, the API returns `409 Conflict`.
- **Performance:** `GET /api/books` is the likely first bottleneck due to wildcard searches, count queries, and offset pagination.
- **Idempotency:** `POST` operations do not support idempotency keys, so retrying after an uncertain outcome may lead to duplicate actions or conflicts.
- **Database Portability:** The implementation uses PostgreSQL-specific features such as partial indexes and `ILIKE`.
- **Testing Coverage:** Integration tests cover main flows but do not fully test concurrency, retries, graceful shutdown, or every catalog operation.

### Lessons Learned

- **Architecture and Trade-offs:** I gained experience combining Vertical Slice and Layered Architecture. I learned that architecture is not about following a specific pattern completely, but about choosing the right abstractions and trade-offs for the project's size and requirements.
- **Concurrency Control:** I learned about optimistic and pessimistic concurrency control and different ways to handle concurrent database operations. In practice, I used a PostgreSQL partial unique index to prevent multiple active loans for the same book.
- **PostgreSQL and Indexes:** I gained a deeper understanding of how B-tree indexes work, how UUID ordering affects index performance, and how PostgreSQL features such as partial indexes and constraints help maintain data integrity.
