# Design Note — Book Catalog Platform (Weeks 1–3)

A living document tracking architectural decisions, trade-offs, and learnings.

---

## 1. What Was Built

- **Week 1:** A CRUD API with separate request/response models, FluentValidation, Swagger, and structured logging. One project contained feature handlers directly dependent on `InMemoryBookStore`.
- **Week 2:** Separate projects, an `IBookRepository` abstraction, shared mapping methods, filtering and pagination, centralized exception handling, and unit tests.
- **Week 3:** PostgreSQL persistence with EF Core migrations and relational entities, database-side querying, transactional lending, borrowing history, and Docker Compose orchestration.

Data is now stored in PostgreSQL and persists across application and container restarts.

---

## 2. Architecture & Solution Structure

The solution uses **Layered Architecture**, with feature-based organization inside the API project:

```text
src/
├── BookCatalog.Domain/          # Entities, shared models, results, queries, repository interfaces
├── BookCatalog.Infrastructure/  # EF Core persistence and PostgreSQL repositories
│   ├── Persistence/             # DbContext, entity configurations, migrations
│   └── Repositories/            # Book, author, user, and loan repositories
└── BookCatalog.Api/             # HTTP endpoints and application setup
    ├── Features/Books/          # Catalog requests, validators, handlers, mappings
    ├── Features/Loans/          # Borrowing, returns, and history
    ├── Filters/                 # ValidationFilter
    └── Exceptions/              # GlobalExceptionHandler
tests/
└── BookCatalog.UnitTests/       # Tests independent of external services
Dockerfile                       # API and migration images
docker-compose.yml               # PostgreSQL, migrations, and API orchestration
```

- **Decision:** Keep `Domain` independent; `Infrastructure` depends on `Domain`, while `Api` references both and registers implementations.
- **Why:** Storage details belong outside the domain and handlers. I kept handlers in `Api` because a separate `Application` project would mainly move existing features into another assembly at this scale.
- **Trade-off:** Application logic and HTTP concerns share a project, although controllers, handlers, and validators have separate responsibilities.

---

## 3. Week 1

### Key Decisions and Rationale

| Decision | Why |
| :--- | :--- |
| Used Vertical Slice Architecture within a single project. | Keeps code organized by feature. As the project grows, this makes it easier to add new features and change existing ones. |
| Used `ConcurrentDictionary<Guid, Book>` registered as a `Singleton`. | It supports thread-safe dictionary operations for simultaneous HTTP requests and provides fast data lookups. The `Singleton` lifetime ensures data is preserved in memory for the entire time the application is running. |
| Generated IDs using `Guid.CreateVersion7()`. | Creates IDs directly in code with a very low risk of collisions. Their time-based structure can help reduce database index fragmentation compared with random IDs when migrating to SQL, depending on how the database sorts GUIDs. |
| Implemented FluentValidation using a custom `ValidationFilter`. | Separates concerns by keeping request models clean and provides more validation features and flexibility than standard DataAnnotations. The filter ensures validation errors use a consistent, standardized `ProblemDetails` format. |
| Store methods return `Task<T>` via `Task.FromResult` without using the `async` keyword internally. | Prepares the system for Week 3 by establishing an async contract across handlers without changing the calling style later. Omitting the `async` keyword in the store avoids generating state machines for in-memory operations. |

### What I Found Hard

- **Choosing the Architecture:** Finding the right balance between avoiding overengineering for Week 1 while still setting up a solid foundation for the project to grow.
- **Sticking to YAGNI:** Fighting the urge to add "just-in-case" features (extra audit fields, soft delete, premature layers) and focusing strictly on current requirements.

### What I Would Improve with More Time

1. **Automated Integration Tests:** Use `WebApplicationFactory` to check routing, status codes, and validation through HTTP requests. Still planned.
2. **Querying Enhancements:** Add pagination with total count and filtering. Completed in Week 2.

---

## 4. Week 2

### Key Decisions and Rationale

| Decision | Why |
| :--- | :--- |
| Separated the solution into `Domain`, `Infrastructure`, and `Api`, keeping feature handlers in `Api`. | Separates responsibilities: `Domain` contains models and interfaces without depending on other projects, `Infrastructure` handles storage, and `Api` contains endpoints, business logic, configuration, and validation. A separate `Application` project would mainly move the existing handlers away from the controllers, so it was not needed at this stage. |
| Introduced `IBookRepository` and replaced direct dependencies on `InMemoryBookStore`. | Hides storage details from handlers and gives them methods for reading and changing books. Prepares the system for a database implementation by allowing the repository and its DI registration to change without rewriting handlers, as long as the contract stays the same. |
| Extracted mapping logic into `BookMappings` extension methods. | Removes repeated mapping code from handlers and keeps conversions in one place. Preserves the separation between API contracts and the internal model, and makes mapping easier to test. |
| Implemented centralized exception handling using `GlobalExceptionHandler`. | Ensures unexpected errors return a consistent `500` ProblemDetails response with a generic message and `traceId`. Logs the original exception for debugging while preventing internal messages and stack traces from reaching the client. |
| Added filtering by title, author, and publication year, with pagination through `PagedResult<T>`. | Allows clients to narrow down the catalog and move through pages. Applying filters before pagination ensures the total count includes all matching books, while the response contains only the requested page and its metadata. |

### Testing Strategy

- **Decision:** Use xUnit, NSubstitute, and Shouldly to check behavior without a database, running API, or external services.
- **Handlers:** Check operation results, selected repository arguments, and missing-book scenarios. Repository substitutes keep these tests independent of storage, meaning none of these unit tests will break when swapping to a real database in Week 3.
- **Mappings and Pagination Models:** Check field conversions separately, plus page calculations and metadata preservation in `PagedResult<T>`. New fields still require updating test expectations.
- **Validators and Exceptions:** Check valid inputs, invalid values, and selected boundaries. Verify the exception handler's response fields and absence of internal exception details.
- **Scope:** I prioritized these behaviors over logging assertions and explicit exception-propagation tests rather than pursuing 100% coverage. Repository filtering and page selection are not tested directly; handler tests only verify parameter forwarding and mapping. HTTP pipeline and database integration tests remain future work.

### What I Found Hard

- **Choosing What to Test:** It was hard to decide which scenarios to cover and what to check inside each test. I had to decide where useful checks ended and unnecessary tests for every imaginable failure began.

  > Refactoring Observation: The restructuring was mostly mechanical, but modifying every handler constructor to introduce `IBookRepository` showed that depending on a concrete store class made the code less flexible to change.

### What I Would Improve with More Time

1. **ISBN Validation:** Validate format and check digits instead of only limiting the length of non-blank values.
2. **Continuous Integration:** Automatically build and run unit tests for pull requests.
3. **Integration Tests:** In a later stage, verify HTTP routing, validation, exception handling, and interaction with a real database.

---

## 5. Week 3

### Data Model

- **Author → Books:** One author has many books, while each book has one required author. Keeping authors separate avoids repeating names in every book row and gives the relationship a database-enforced foreign key.
- **Book → Loans:** One book has many loans over time. `ReturnedAt = null` identifies the active loan, while completed rows remain available as borrowing history.
- **User → Loans:** One user can borrow many books over time. `Loan` connects a user and a book while storing relationship-specific data such as `BorrowedAt` and `ReturnedAt`.
- **Loan identity:** `Loan` has its own primary key because the same user can borrow the same book multiple times. Using only `BookId` and `UserId` as a composite key would prevent repeated loans and would not identify one borrowing event independently.
- **Active loan count:** `User.ActiveLoanCount` is a denormalized counter updated in the same transaction as the loan. It provides a multi-step transactional operation and avoids recounting the user's borrowing history when the current count is needed.

Database constraints protect invariants independently of application validation: publication years cannot be less than one, active-loan counts cannot be negative, return time cannot precede borrow time, foreign keys prevent orphaned relations, and a partial unique index allows only one active loan per book.

### Dockerfile Structure

| Line | Purpose |
| :--- | :--- |
| `FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build` | Starts a build stage with the full .NET SDK. |
| `WORKDIR /src` | Sets the working directory for restore and publish commands. |
| `COPY` project files, then `dotnet restore` | Restores dependencies in a cacheable layer that changes only when project files change. |
| `COPY . .` | Copies the remaining source code into the build stage. |
| `dotnet publish ... --no-restore` | Compiles the API in Release mode and places deployable files in `/app/publish`. |
| `FROM build AS migration` | Starts a migration stage from the SDK build stage; Compose builds it as a separate image. |
| `dotnet tool install ... dotnet-ef` | Installs the EF Core CLI version used by the project. |
| Migration `ENTRYPOINT` | Applies committed migrations and exits; Compose starts the API only after a successful exit. |
| `FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final` | Starts a clean runtime-only stage without the SDK or source code. |
| `ENV ASPNETCORE_HTTP_PORTS=8080` and `EXPOSE 8080` | Configures Kestrel's container port and documents it for Docker. |
| `COPY --from=build /app/publish .` | Copies only the published application into the final image. |
| `USER $APP_UID` | Runs the API as the non-root user provided by the .NET image. |
| API `ENTRYPOINT` | Starts `BookCatalog.Api.dll` when the API container runs. |

### Database Choice: PostgreSQL

I chose PostgreSQL as the relational storage backend for 4 specific reasons:

1. **Lightweight for Docker:** The `postgres:17-alpine` image is relatively small, reducing download time and local setup overhead.
2. **UUIDv7-Friendly Ordering:** PostgreSQL stores UUIDs in sortable binary form, allowing our `Guid.CreateVersion7()` keys to provide more sequential B-Tree insertions and reduce index fragmentation compared with random UUIDv4 values.
3. **Partial Unique Indexes:** It supports conditional indexes (e.g., `UNIQUE(BookId) WHERE ReturnedAt IS NULL`), elegantly solving the double-borrowing race condition at the database level.
4. **No Licensing Limits:** It is 100% free and open-source, with no artificial caps on database size or hardware usage.

### Impact of Replacing In-Memory Storage

- **Amount of change:** Significant. The change affected entities, repository contracts and implementations, DTOs, mappings, validators, handlers, dependency registration, controllers, and unit tests.
- **Why so much changed:** Replacing the storage alone was relatively isolated. Most changes came from replacing the flat `Book` model with relationships between authors, books, users, and loans.
- **Why Result was introduced:** The expanded domain added expected failures such as missing authors or users, unavailable books, and already returned loans. The Result pattern lets handlers return typed `NotFound` and `Conflict` errors without using exceptions for normal business outcomes. Unexpected database failures still reach `GlobalExceptionHandler` and return `500`.
- **What Week 2 got right:** Handlers already depended on repository interfaces, and unit tests used substitutes. EF Core remained inside `Infrastructure`, and the tests still did not require a real database.
- **What Week 2 revealed:** The abstraction separated storage from business logic, but its contracts were designed around a simple in-memory `Book` model. It handled the persistence replacement well, but it could not prevent a real domain expansion from affecting multiple layers.

### Expected Performance Bottleneck

- **First bottleneck:** `GET /api/books`.
- **Why:** It performs a count and a page query, searches titles and author names with `ILIKE '%value%'`, and uses offset pagination. Leading-wildcard searches may scan many rows, while deep offsets require PostgreSQL to pass over earlier results.
- **Possible improvements:** PostgreSQL trigram indexes, keyset pagination and response projection with `Select`.