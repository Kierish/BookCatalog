# Book Catalog Platform API

REST API for managing a book catalog and lending workflow, built with .NET 10 (C# 14), EF Core, and PostgreSQL.

## Architecture

The solution uses **Layered Architecture**, with feature-based organization inside the API project:

- **`BookCatalog.Domain`**: Domain entities (`Book`, `Author`, `User`, and `Loan`), shared models, results, and repository interfaces. Zero external dependencies.
- **`BookCatalog.Infrastructure`**: EF Core persistence, PostgreSQL repositories, entity configurations, and migrations.
- **`BookCatalog.Api`**: REST endpoints, feature handlers, request models, validation, error handling, and dependency registration.

The separate **`BookCatalog.UnitTests`** project uses xUnit, NSubstitute, and Shouldly to test business logic without accessing a real database.

Data is stored in PostgreSQL and persists across application and container restarts.

## API Endpoints

| Method | Endpoint | Description | Status Codes |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/books` | Create a book | `201`, `400`, `404` |
| `GET` | `/api/books` | Retrieve paginated and filtered books | `200`, `400` |
| `GET` | `/api/books/{id}` | Retrieve a book by ID | `200`, `400`, `404` |
| `PUT` | `/api/books/{id}` | Update a book | `204`, `400`, `404` |
| `DELETE` | `/api/books/{id}` | Delete a book | `204`, `400`, `404` |
| `POST` | `/api/loans` | Borrow an available book | `201`, `400`, `404`, `409` |
| `POST` | `/api/loans/{loanId}/return` | Return a borrowed book | `204`, `400`, `404`, `409` |
| `GET` | `/api/loans` | Retrieve paginated borrowing history | `200`, `400` |

### Book Query Parameters

- `search`: Case-insensitive title and author name search.
- `authorId`: Exact author filter.
- `publicationYear`: Exact publication year filter.
- `pageNumber`: Page number (default: `1`).
- `pageSize`: Items per page (default: `10`, maximum: `50`).

### Loan History Query Parameters

- `bookId`: Exact book filter.
- `userId`: Exact user filter.
- `pageNumber`: Page number (default: `1`).
- `pageSize`: Items per page (default: `10`, maximum: `50`).

Paginated responses include `items`, `totalCount`, `pageNumber`, `pageSize`, `totalPages`, `hasPreviousPage`, and `hasNextPage`.

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

### Run with Docker Compose

Copy `.env.example` to `.env` and set `POSTGRES_PASSWORD`, then run:

```bash
docker compose up --build
```

This starts PostgreSQL, applies EF Core migrations, and starts the API. Database data is persisted in a Docker volume.

Swagger UI is available at [http://localhost:8080/swagger](http://localhost:8080/swagger).

To stop the application:

```bash
docker compose down
```

### Run Tests

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
dotnet test BookCatalog.slnx
```

Additional design decisions and trade-offs are documented in [DESIGN_NOTE.md](DESIGN_NOTE.md).
