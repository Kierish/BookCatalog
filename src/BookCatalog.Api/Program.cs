using BookCatalog.Api.Filters;
using BookCatalog.Domain.Interfaces;
using BookCatalog.Features.Books.CreateBook;
using BookCatalog.Features.Books.DeleteBook;
using BookCatalog.Features.Books.GetBookById;
using BookCatalog.Features.Books.GetBooksList;
using BookCatalog.Features.Books.UpdateBook;
using BookCatalog.Infrastructure.Storage;
using FluentValidation;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>(); 
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationRulesToSwagger();
builder.Services.AddProblemDetails();

// Dependency Injection
builder.Services.AddSingleton<IBookRepository, InMemoryBookRepository>();
builder.Services.AddScoped<GetBookByIdHandler>();
builder.Services.AddScoped<GetBooksListHandler>();
builder.Services.AddScoped<CreateBookHandler>();
builder.Services.AddScoped<UpdateBookHandler>();
builder.Services.AddScoped<DeleteBookHandler>();

var app = builder.Build();

// HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
