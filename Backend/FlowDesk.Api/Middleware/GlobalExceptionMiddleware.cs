using FlowDesk.Application.Common.DTOs;
using FlowDesk.Application.Common.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace FlowDesk.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var traceId = context.TraceIdentifier;

            // 🔥 Structured Logging (VERY IMPORTANT)
            _logger.LogError(ex,
                "Exception Occurred | TraceId: {TraceId} | Path: {Path} | Method: {Method} | User: {User}",
                traceId,
                context.Request.Path,
                context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous"
            );

            var response = new ErrorResponseDto
            {
                TraceId = traceId
            };

            switch (ex)
            {
                case AppException appEx:
                    response.StatusCode = appEx.StatusCode;
                    response.Message = appEx.Message;
                    break;

                //case ValidationException validationEx:
                //    response.StatusCode = 400;
                //    response.Message = "Validation failed";
                //    response.Errors = validationEx.Errors
                //        .Select(e => e.ErrorMessage)
                //        .ToList();

                //    _logger.LogWarning("Validation failed | TraceId: {TraceId} | Errors: {@Errors}",
                //        traceId, response.Errors);
                //    break;

                case UnauthorizedAccessException:
                    response.StatusCode = 401;
                    response.Message = "Unauthorized access";
                    break;

                case KeyNotFoundException:
                    response.StatusCode = 404;
                    response.Message = "Resource not found";
                    break;

                default:
                    response.StatusCode = 500;
                    response.Message = "Something went wrong";

                    if (_env.IsDevelopment())
                    {
                        response.Details = ex.ToString(); // full stack trace in dev
                    }
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.StatusCode;

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
