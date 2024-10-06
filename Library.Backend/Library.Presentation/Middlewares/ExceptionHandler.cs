using Library.Domain.Results;

namespace Library.Presentation.Middlewares;

public class ExceptionHandler : IMiddleware
{
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);
		}
		catch (Exception exception)
		{
			//TODO Handle some logging logic
			context.Response.Headers.Accept = "application/json";
			await context.Response.WriteAsJsonAsync(new EmptyErrorResult("An internal error occurred while processing your request"));
		}
	}
}