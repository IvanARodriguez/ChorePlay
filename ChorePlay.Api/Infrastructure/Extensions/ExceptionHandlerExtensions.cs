using ChorePlay.Api.Infrastructure.Middlewares;

namespace ChorePlay.Api.Infrastructure.Extensions
{
    public static class ExceptionHandlerExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandler>();
        }
    }
}