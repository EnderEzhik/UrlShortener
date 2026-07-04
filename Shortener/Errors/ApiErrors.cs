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