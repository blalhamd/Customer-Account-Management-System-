using CAMS.API.MiddleWares.Models;
using CAMS.Shared.Exceptions;
using System.Net;
using System.Text.Json;

namespace CAMS.API.MiddleWares
{
    public class ErrorHandlingMiddleWare
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleWare(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // as Log
                await HandleError(ex, context);
            }
        }

        private async Task HandleError(Exception ex, HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new CustomResponse()
            {
                Message = ex.Message,
                Details = ex.InnerException?.Message
            };

            switch (ex)
            {
                case ItemNotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case BadRequestException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                case ItemAlreadyExistException:
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;
                case InvalidOperationException:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;
                default:
                    break;
            }

            var json = JsonSerializer.Serialize(response);

            await context.Response.WriteAsync(json);
        }
    }
}
