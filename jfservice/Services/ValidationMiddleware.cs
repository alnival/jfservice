using Newtonsoft.Json;

namespace jfservice.Services
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = new
            {
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Title = "Произошла ошибка валидации на уровне мидлвар.",
                Status = 400,
                Errors = new Dictionary<string, List<string>>
                    {
                        { "Error", new List<string> { exception.Message } }
                    }
            };

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}