using BookCatalog.Api.Exceptions;
using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Api.Features.Books.DeleteBook;
using BookCatalog.Api.Features.Books.GetBookById;
using BookCatalog.Api.Features.Books.GetBooksList;
using BookCatalog.Api.Features.Books.UpdateBook;
using BookCatalog.Api.Features.Loans.BorrowBook;
using BookCatalog.Api.Features.Loans.GetLoanHistory;
using BookCatalog.Api.Features.Loans.ReturnBook;
using BookCatalog.Api.Filters;
using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Persistence;
using BookCatalog.Infrastructure.Repositories;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers & Validation
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>(); 
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Swagger
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationRulesToSwagger();

// Exception Handling
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Database
var connectionString = builder.Configuration
    .GetConnectionString("BookCatalog")
    ?? throw new InvalidOperationException(
        "Connection string 'BookCatalog' is not configured.");

builder.Services.AddDbContext<BookCatalogDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<BookCatalogDbContext>(name: "postgresql", tags: ["ready"]);

// Dependency Injection

// Time Abstraction
builder.Services.AddSingleton(TimeProvider.System);

// Repositories 
builder.Services.AddScoped<IAuthorRepository, AuthorRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

// Feature Handlers (Book Catalog)
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<GetBooksListHandler>();
builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddScoped<DeleteBookHandler>();

// Feature Handlers (Lending)
builder.Services.AddScoped<BorrowBookHandler>();
builder.Services.AddScoped<ReturnBookHandler>();
builder.Services.AddScoped<GetLoanHistoryHandler>();


var app = builder.Build();

// HTTP request pipeline

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live",
    new HealthCheckOptions { Predicate = _ => false });

app.MapHealthChecks("/health/ready",
    new HealthCheckOptions { Predicate = healthCheck => healthCheck.Tags.Contains("ready") });

app.Run();

public partial class Program
{
}
