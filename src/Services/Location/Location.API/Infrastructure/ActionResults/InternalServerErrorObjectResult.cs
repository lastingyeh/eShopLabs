using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopLabs.Services.Location.API.Infrastructure.ActionResults
{
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}