using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FCS.Utilities.Middleware;

/// <summary>
/// Add Global Exception Handling Middleware
/// </summary>
/// <param name="logger"></param>
public sealed class ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger) : IMiddleware
{
	private readonly ILogger<ExceptionHandlerMiddleware> _logger = logger;

	/// <summary>
	/// Invoke next item in pipeline. Report Error if required.
	/// </summary>
	/// <param name="context">Request Context</param>
	/// <param name="next">Next Item In Pipeline</param>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);                        // Pass To Next In Pipeline
		}
		catch(Exception ex)
		{
			await HandleExceptionAsync(context, ex);    // Report Error
		}
	}

	/// <summary>
	/// Report Error
	/// </summary>
	/// <param name="context">Http Context</param>
	/// <param name="exception">Exception Object</param>
	private Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		//ErrorHandler Err = new();
		//Tuple<bool, string> Status = Err.ReportError(exception, context);

		_logger.LogCritical(exception, exception.Message);  // Log Error

		//string result = ProcessControl.IsReleaseBuild() ? "An unexpected fault happened. Try again later." : exception.Message;

		//context.Response.ContentType = MediaTypeNames.Application.Json;
		//context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

		//return context.Response.WriteAsync(result);         // Inform User

		return Task.CompletedTask;
	}
}

/// <summary>
/// Exception Handler Middleware Extensions
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
	/// <summary>
	/// Add Global Exception Handling To Pipeline
	/// </summary>
	/// <param name="builder">WebApplicationBuilder</param>
	public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder) => builder.UseMiddleware<ExceptionHandlerMiddleware>();
}