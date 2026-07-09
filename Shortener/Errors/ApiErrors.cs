using Microsoft.AspNetCore.Mvc;

namespace Shortener.Errors;

public sealed record ApiError(string Type, string Title, int StatusCode, string Detail);

public static class ApiErrors
{
    public static readonly ApiError IncorrectUrl = new(
        Type: "errors/incorrect-url",
        Title: "Incorrect url",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "Url should start with 'https://' or 'http://'"
    );

    public static readonly ApiError IncorrectUrlLength = new(
        Type: "errors/incorrect-url-length",
        Title: "Incorrect url length",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "Url length should be greater than or equal to 4 and less than or equal to 1000"
    );

    public static readonly ApiError IncorrectExpirationDate = new(
        Type: "errors/incorrect-expiration-date",
        Title: "Incorrect expiration date",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "Expiration date should be in the future"
    );

    public static readonly ApiError IncorrectPageNumber = new(
        Type: "errors/incorrect-page-number",
        Title: "Incorrect page number",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "Page number should be greater than 0"
    );

    public static readonly ApiError IncorrectPageSize = new(
        Type: "errors/incorrect-page-size",
        Title: "Incorrect page size",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "The page size should be greater than or equal to 1 and less than or equal to 100"
    );

    public static readonly ApiError IncorrectShortCode = new(
        Type: "errors/incorrect-short-code",
        Title: "Incorrect short code",
        StatusCode: StatusCodes.Status404NotFound,
        Detail: "Url with this short code was not found or you do not have access rights to this object"
    );

    public static readonly ApiError IncorrectOrExpiredShortCode = new(
        Type: "errors/incorrect-or-expired-short-code",
        Title: "Incorrect or expired-short code",
        StatusCode: StatusCodes.Status404NotFound,
        Detail: "Short url with this short code not found or expired"
    );

    public static readonly ApiError InternalServerError = new(
        Type: "errors/internal-server-error",
        Title: "Internal server error",
        StatusCode: StatusCodes.Status500InternalServerError,
        Detail: "An unexpected error occurred"
    );

    public static readonly ApiError IncorrectLoginLength = new(
        Type: "errors/incorrect-login-length",
        Title: "Incorrect login length",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "Login length should be greater than or equal to 4 and less than or equal to 20"
    );

    public static readonly ApiError IncorrectPasswordLength = new(
        Type: "errors/incorrect-password-length",
        Title: "Incorrect password length",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "Password length should be greater than or equal to 8 and less than or equal to 64"
    );

    public static readonly ApiError IncorrectLoginOrPassword = new(
        Type: "errors/incorrect-login-or-password",
        Title: "Incorrect login or password",
        StatusCode: StatusCodes.Status400BadRequest,
        Detail: "Incorrect login or password"
    );

    public static readonly ApiError LoginIsAlreadyInUse = new(
        Type: "errors/login-is-already-in-use",
        Title: "Login is already in use",
        StatusCode: StatusCodes.Status409Conflict,
        Detail: "Login is already in use"
    );
}

public static class ControllerBaseExtensions
{
    public static ObjectResult Problem(this ControllerBase controller, ApiError error)
        => controller.Problem(
            title: error.Title,
            detail: error.Detail,
            statusCode: error.StatusCode,
            type: error.Type);
}