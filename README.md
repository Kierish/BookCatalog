# Book Catalog Platform API

REST API for managing a book catalog platform built with .NET 10 (C# 14).

## Architecture
The application is built using **Vertical Slice Architecture (VSA)** grouped by business capabilities:
- In-memory thread-safe state storage (`ConcurrentDictionary`).
- Native ASP.NET Core Dependency Injection with method injection (`[FromServices]`).
- Automated request validation via FluentValidation and custom `ValidationFilter`.
- Consistent RFC 7807 `ProblemDetails` error reporting.
- Structured logging with built-in `ILogger`.

## API Endpoints

| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/books` | Create a new book | `201 Created`, `400 Bad Request` |
| `GET` | `/api/books` | Retrieve all books | `200 OK` |
| `GET` | `/api/books/{id}` | Retrieve book by ID | `200 OK`, `404 Not Found` |
| `PUT` | `/api/books/{id}` | Update existing book | `204 NoContent`, `400 Bad Request`, `404 Not Found` |
| `DELETE`| `/api/books/{id}` | Remove book from catalog | `204 NoContent`, `404 Not Found` |

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run the Application
```bash
dotnet restore
dotnet run --project src/BookCatalog
```

### API Documentation
Once running, navigate to Swagger UI:
- `https://localhost:7145/swagger` 

