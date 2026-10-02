using BookCatalog.Api.Exceptions;
using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Api.Features.Books.DeleteBook;
using BookCatalog.Api.Features.Books.GetBookById;
using BookCatalog.Api.Features.Books.GetBooksList;
using BookCatalog.Api.Features.Books.UpdateBook;
using BookCatalog.Api.Filters;
using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Repositories;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

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

// Dependency Injection
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<GetBooksListHandler>();
builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddScoped<DeleteBookHandler>();

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

app.Run();
