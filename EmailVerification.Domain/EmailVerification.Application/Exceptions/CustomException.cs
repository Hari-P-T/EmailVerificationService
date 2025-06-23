using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Integrate.EmailVerification.Api.Middlewares
{
    [ExcludeFromCodeCoverage]
    public abstract class CustomException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        protected CustomException(string message, HttpStatusCode statusCode)
            : base(message) => StatusCode = statusCode;

        protected CustomException(string message, Exception innerException, HttpStatusCode statusCode)
            : base(message, innerException) => StatusCode = statusCode;
    }
    [ExcludeFromCodeCoverage]
    public class MethodFailException : CustomException
    {
        public MethodFailException(string message)
            : base(message, HttpStatusCode.InternalServerError) { }

        public MethodFailException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.InternalServerError) { }
    }
    [ExcludeFromCodeCoverage]
    public class CheckValidationException : CustomException
    {
        public CheckValidationException(string message)
            : base(message, HttpStatusCode.BadRequest) { }

        public CheckValidationException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.BadRequest) { }
    }
    [ExcludeFromCodeCoverage]
    public class NotFoundException : CustomException
    {
        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound) { }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.NotFound) { }
    }
    [ExcludeFromCodeCoverage]
    public class NullFoundException : CustomException
    {
        public NullFoundException(string message)
            : base(message, HttpStatusCode.BadRequest) { }

        public NullFoundException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.BadRequest) { }
    }
}
