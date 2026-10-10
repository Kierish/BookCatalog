using BookCatalog.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected ObjectResult HandleFailure(Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException(
                    "Cannot handle a successful result as a failure.");
            }

            var statusCode = result.Error.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            return Problem(
                statusCode: statusCode,
                title: result.Error.Code,
                detail: result.Error.Message);
        }
    }
}
