# Design Note — Book Catalog Platform (Week 1)

A living document tracking architectural decisions, trade-offs, and learnings.

---

## 1. What Was Built
A foundational RESTful API for a book catalog supporting full CRUD operations:
- `POST /api/books` — Creates a book (`201 Created` with `Location` header). Validated payload.
- `GET /api/books` — Returns all books (`200 OK`).
- `GET /api/books/{id}` — Returns a book by ID (`200 OK`, `404 Not Found`).
- `PUT /api/books/{id}` — Updates book metadata (`204 NoContent`, `400 Bad Request`, `404 Not Found`).
- `DELETE /api/books/{id}` — Deletes a book (`204 NoContent`, `404 Not Found`).

Data is persisted in-memory via `InMemoryBookStore` until the database migration in Week 3.

---

## 2. Architecture & Solution Structure
The project uses a **Feature-First (Vertical Slice) Architecture (VSA)** in a single cohesive project:

```text
src/BookCatalog/
├── Domain/
│   └── Book.cs                 # Entity with sequential Guid V7
├── Features/Books/
│   ├── CreateBook/         # Request, Validator, Handler
│   ├── DeleteBook/         # Handler
│   ├── GetBookById/        # Handler
│   ├── GetBooksList/       # Handler
│   ├── UpdateBook/         # Request, Validator, Handler
│   ├── BookResponse.cs     # Shared response record contract
│   └── BooksController.cs  # REST routing & status code mapping
└── Infrastructure/
    ├── Filters/
    │   └── ValidationFilter.cs # Pipeline action filter for FluentValidation
    └── Storage/
        └── InMemoryBookStore.cs# In-memory storage implementation
```

---

## 3. Key Decisions and Rationale

### 3.1. Vertical Slices over N-Tier / Monolith
- **Decision:** Used Vertical Slice Architecture instead of a traditional N-Tier monolith.
- **Why:** Keeps features completely independent of each other. As the project grows, this makes it very easy to add new features without worrying about breaking existing code.

### 3.2. Thread-Safe In-Memory Store (ConcurrentDictionary + Singleton)
- **Decision:** Used `ConcurrentDictionary<Guid, Book>` registered as a `Singleton`.
- **Why:** It is thread-safe for simultaneous HTTP requests and provides fast, performant data lookups. The `Singleton` lifetime ensures data is preserved in memory for the entire time the application is running.

### 3.3. Sequential UUIDv7 for Identifiers
- **Decision:** Generated IDs using `Guid.CreateVersion7()`.
- **Why:** Creates collision-free IDs directly in code. Because they are sorted by time, they won't cause performance issues with database indexes when migrating to SQL.

### 3.4. FluentValidation with Custom Action Filter
- **Decision:** Implemented FluentValidation using a custom `ValidationFilter`.
- **Why:** 
  1. Separates concerns by keeping request models clean and gives far more validation features and flexibility compared to standard DataAnnotations.
  2. Avoids using the deprecated automatic validation package and ensures the API returns a consistent, standardized `ProblemDetails` error format.

### 3.5. Asynchronous Persistence Contracts for In-Memory Store
- **Decision:** Store methods return `Task` / `Task<T>` (via `Task.CompletedTask` and `Task.FromResult`) without using the `async` keyword internally.
- **Why:** Prepares the system for Week 3 (EF Core) by establishing an async contract across handlers without breaking method signatures later. Simultaneously, omitting the `async` keyword in the store avoids generating state machine overhead for in-memory operations.

---

## 4. What I Found Hard
- **Choosing the Architecture:** Finding the right balance between avoiding overengineering for Week 1 while still setting up a solid foundation for the project to grow.
- **Sticking to YAGNI:** Fighting the urge to add "just-in-case" features (extra audit fields, soft delete, premature layers) and focusing strictly on current requirements.
---

## 5. What I Would Improve with More Time
1. **Automated Integration Tests:** Add API tests using `WebApplicationFactory` to verify routing, status codes, and validation pipelines under real HTTP calls.
2. **Querying Enhancements:** Implement pagination (with total count) and filtering on `GET /api/books`.