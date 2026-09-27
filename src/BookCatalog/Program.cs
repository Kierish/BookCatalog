using BookCatalog.Features.Books.CreateBook;
using BookCatalog.Infrastructure.Filters;
using BookCatalog.Infrastructure.Storage;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>(); 
});
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

// Dependency Injection
builder.Services.AddSingleton<InMemoryBookStore>();
builder.Services.AddScoped<CreateBookHandler>();

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
