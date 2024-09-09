using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Personal.Application;
using Personal.Application.Exceptions;

namespace Personal.Api.Filters;

/// <summary>
/// Exception handling class
/// </summary>
public class ExceptionsFilter : IExceptionFilter
{

    /// <summary>
    /// Called when an exception is raised
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is PersonalException)
        {
            HandlePersonalException(context);
        }
        else
        {
          ThrowUnknownError(context);
        }
    }
    
    private static void HandlePersonalException(ExceptionContext context)
    {
        if (context.Exception is ValidationErrorException)
        {
            HandleErrorsValidationException(context);
        }
    }

    /// <summary>
    /// Handles the validation errors exception by setting the response status code to 400 (Bad Request)
    /// and returning a BaseResponseError object with the error messages.
    /// </summary>
    /// <param name="context">The ExceptionContext object containing information about the exception and the current context.</param>
    private static void HandleErrorsValidationException(ExceptionContext context)
    {
        var validationErrorsException = context.Exception as ValidationErrorException;
        
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Result = new ObjectResult(new BaseResponseError(validationErrorsException?.ErrorMessages));
    }


    /// <summary>
    /// Throws an unknown error by setting the response status code to internal server error and
    /// providing a base response error object with a default error message.
    /// </summary>
    /// <param name="context">The exception context.</param>
    private static void ThrowUnknownError(ExceptionContext context)
    {
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Result = new ObjectResult(new BaseResponseError(context.Exception.Message));
    }
}
