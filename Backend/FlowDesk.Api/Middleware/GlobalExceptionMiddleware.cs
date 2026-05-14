using FlowDesk.Domain.DTOs;
using FlowDesk.Application.Common.Exceptions;
using System.ComponentModel.DataAnnotations;

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

            var response = new ErrorResponseDto
            {
                TraceId = traceId
            };

            switch (ex)
            {
                // PRIMARY FLOW
                case AppException appEx:
                    response.StatusCode = appEx.StatusCode;
                    response.Message = appEx.Message;
                    response.ErrorCode = appEx.ErrorCode;
                    response.Details = appEx.Details;

                    if (appEx is ValidationAppException && appEx.Details is IEnumerable<string> errors)
                    {
                        response.Errors = errors.ToList();
                    }

                    LogByStatusCode(appEx.StatusCode, ex, context, traceId);
                    break;

                // DataAnnotations / FluentValidation
                case ValidationException validationEx:
                    response.StatusCode = 400;
                    response.Message = "Validation failed";
                    response.ErrorCode = "VALIDATION_ERROR";
                    response.Errors = new List<string> { validationEx.Message };

                    _logger.LogWarning(ex,
                        "Validation failed | TraceId: {TraceId} | Path: {Path}",
                        traceId, context.Request.Path);
                    break;

              
                case ArgumentException argEx:
                    response.StatusCode = 400;
                    response.Message = argEx.Message;
                    response.ErrorCode = "BAD_REQUEST";

                    _logger.LogWarning(ex,
                        "Argument exception | TraceId: {TraceId} | Path: {Path}",
                        traceId, context.Request.Path);
                    break;

                case InvalidOperationException invalidOpEx:
                    response.StatusCode = 400;
                    response.Message = invalidOpEx.Message;
                    response.ErrorCode = "INVALID_OPERATION";

                    _logger.LogWarning(ex,
                        "Invalid operation | TraceId: {TraceId} | Path: {Path}",
                        traceId, context.Request.Path);
                    break;

                case UnauthorizedAccessException:
                    response.StatusCode = 401;
                    response.Message = "Unauthorized";
                    response.ErrorCode = "UNAUTHORIZED";

                    _logger.LogWarning(ex,
                        "Unauthorized access | TraceId: {TraceId}",
                        traceId);
                    break;

                case KeyNotFoundException:
                    response.StatusCode = 404;
                    response.Message = "Resource not found";
                    response.ErrorCode = "NOT_FOUND";

                    _logger.LogWarning(ex,
                        "Resource not found | TraceId: {TraceId}",
                        traceId);
                    break;

                default:
                    response.StatusCode = 500;
                    response.Message = "Something went wrong";
                    response.ErrorCode = "INTERNAL_SERVER_ERROR";

                    _logger.LogError(ex,
                        "Unhandled exception | TraceId: {TraceId} | Path: {Path} | Method: {Method}",
                        traceId,
                        context.Request.Path,
                        context.Request.Method);

                    if (_env.IsDevelopment())
                    {
                        response.Details = ex.ToString();
                    }
                    break;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = response.StatusCode;

            await context.Response.WriteAsJsonAsync(response);
        }

        private void LogByStatusCode(int statusCode, Exception ex, HttpContext context, string traceId)
        {
            var path = context.Request.Path;
            var method = context.Request.Method;

            if (statusCode >= 500)
            {
                _logger.LogError(ex,
                    "Server error | TraceId: {TraceId} | Path: {Path} | Method: {Method}",
                    traceId, path, method);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(ex,
                    "Client error | StatusCode: {StatusCode} | TraceId: {TraceId} | Path: {Path}",
                    statusCode, traceId, path);
            }
        }
    }
}
