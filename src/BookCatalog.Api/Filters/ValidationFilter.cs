using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookCatalog.Api.Filters
{
    public sealed class ValidationFilter : IAsyncActionFilter
    {
        private readonly ILogger<ValidationFilter> _logger;

        public ValidationFilter(ILogger<ValidationFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values.Where(v => v != null))
            {
                var argumentType = argument!.GetType();

                if (argumentType.IsValueType || argumentType == typeof(string))
                    continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

                if (validator is not null)
                {
                    var validationContext = new ValidationContext<object>(argument);

                    var validationResult = await validator.ValidateAsync(
                        validationContext,
                        context.HttpContext.RequestAborted);

                    if (!validationResult.IsValid)
                    {
                        _logger.LogWarning(
                            "Validation failed for {RequestType}",
                            argumentType.Name);

                        var problemDetails = new HttpValidationProblemDetails(
                            validationResult.Errors
                                .GroupBy(e => e.PropertyName)
                                .ToDictionary(
                                    g => g.Key,
                                    g => g.Select(e => e.ErrorMessage).ToArray()
                                )
                        )
                        {
                            Status = StatusCodes.Status400BadRequest,
                            Title = "Validation Failed",
                            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
                            Detail = "One or more validation errors occurred.",
                            Instance = context.HttpContext.Request.Path,
                        };

                        context.Result = new BadRequestObjectResult(problemDetails);
                        return;
                    }
                }
            }

            await next();
        }
    }
}
