using System;
using System.Collections.Generic;
using System.Text;

namespace FlowDesk.Application.Common.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public object? Details { get; }

        public AppException(
            string message,
            int statusCode = 400,
            string errorCode = "APP_ERROR",
            object? details = null)
            : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details;
        }
    }
    public class NotFoundException : AppException
    {
        public NotFoundException(string message = "Resource not found")
            : base(message, 404, "NOT_FOUND") { }
    }

    public class UnauthorizedException : AppException
    {
        public UnauthorizedException(string message = "Unauthorized")
            : base(message, 401, "UNAUTHORIZED") { }
    }

    public class ForbiddenException : AppException
    {
        public ForbiddenException(string message = "Forbidden")
            : base(message, 403, "FORBIDDEN") { }
    }

    public class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message, 400, "BAD_REQUEST") { }
    }

    public class ConflictException : AppException
    {
        public ConflictException(string message)
            : base(message, 409, "CONFLICT") { }
    }

    public class ValidationAppException : AppException
    {
        public ValidationAppException(object errors)
            : base("Validation failed", 400, "VALIDATION_ERROR", errors) { }
    }

    public class InternalServerException : AppException
    {
        public InternalServerException(string message = "Internal server error")
            : base(message, 500, "INTERNAL_SERVER_ERROR") { }
    }
}
