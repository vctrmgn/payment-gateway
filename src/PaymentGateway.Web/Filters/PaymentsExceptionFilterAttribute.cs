using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PaymentGateway.SharedKernel.Exceptions;

namespace PaymentGateway.Web.Filters;

public class PaymentsExceptionFilterAttribute : TypeFilterAttribute
{
    public PaymentsExceptionFilterAttribute()
        : base(typeof(ExceptionHandlerAttribute))
    {
    }

    private class ExceptionHandlerAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ExceptionHandlerAttribute> _logger;
    
        public ExceptionHandlerAttribute(ILogger<ExceptionHandlerAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is not {} ex) 
                return;

            switch (ex)
            {
                case ResourceNotFoundException:
                    LogException(LogLevel.Warning, ex);
                    context.Result = BuildObjectResult(ex.Message, StatusCodes.Status404NotFound);
                    return;
                case ValidationException:
                    LogException(LogLevel.Debug, ex);
                    context.Result = BuildObjectResult(ex.Message, StatusCodes.Status422UnprocessableEntity);
                    return;
                case DuplicateProcessingException:
                    LogException(LogLevel.Warning, ex);
                    context.Result = BuildObjectResult(ex.Message, StatusCodes.Status429TooManyRequests);
                    return;
                default:
                    LogException(LogLevel.Error, ex);
                    context.Result = BuildObjectResult(ex.Message, StatusCodes.Status500InternalServerError);
                    break;
            }
        }
    
        private void LogException(LogLevel logLevel, Exception ex)
        {
            _logger.Log(logLevel, "{ExMessage}", ex.Message);
        }

        private static ObjectResult BuildObjectResult(string message, int statusCode)
        {
            return new ObjectResult(message)
            {
                StatusCode = statusCode
            };
        }
    }
}