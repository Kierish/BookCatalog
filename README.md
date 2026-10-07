# Book Catalog Platform API

REST API for managing a book catalog platform built with .NET 10 (C# 14).

## Architecture

The solution uses **Layered Architecture**, with feature-based organization inside the API project:

- **`BookCatalog.Domain`**: Core domain entities (`Book`), common domain models (`PagedResult<T>`), and repository interfaces (`IBookRepository`). Zero external dependencies.
- **`BookCatalog.Infrastructure`**: Persistence implementations (`InMemoryBookRepository` backed by thread-safe `ConcurrentDictionary`).
- **`BookCatalog.Api`**: REST endpoints, feature handlers (`CreateBook`, `GetBooksList`, etc.), request models, FluentValidation rules, and centralized exception handling.

The separate **`BookCatalog.UnitTests`** project uses xUnit, NSubstitute, and Shouldly to test business logic, domain models, mappings, validators, and error sanitization.

Data is stored in memory and is lost on restart.

## API Endpoints

| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/books` | Create a new book | `201 Created`, `400 Bad Request` |
| `GET` | `/api/books` | Retrieve paginated & filtered books | `200 OK`, `400 Bad Request` |
| `GET` | `/api/books/{id}` | Retrieve book by ID | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `PUT` | `/api/books/{id}` | Update existing book | `204 No Content`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/books/{id}` | Remove book from catalog | `204 No Content`, `400 Bad Request`, `404 Not Found` |

Invalid book ID formats return `400 Bad Request`.

### Query Parameters for `GET /api/books`

- `title` *(string, optional)*: Case-insensitive substring filter.
- `author` *(string, optional)*: Case-insensitive substring filter.
- `publicationYear` *(int, optional)*: Exact publication year filter (`1` to current year).
- `pageNumber` *(int, optional, default: 1)*: Page number (`>= 1`).
- `pageSize` *(int, optional, default: 10)*: Number of items per page (`1` to `50`).

List responses include `items`, `totalCount`, `pageNumber`, `pageSize`, `totalPages`, `hasPreviousPage`, and `hasNextPage`. `totalCount` is the number of books matching the filters before pagination.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the Application

```bash
dotnet restore
dotnet run --project src/BookCatalog.Api --launch-profile https
```

### Run Tests

```bash
dotnet test
```

### API Documentation

Once running, navigate to [Swagger UI](https://localhost:7145/swagger).
