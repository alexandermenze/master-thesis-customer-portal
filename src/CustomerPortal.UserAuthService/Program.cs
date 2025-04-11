using System.Net;
using CustomerPortal.UserAuthService.Domain.Exceptions;
using CustomerPortal.UserAuthService.Domain.Extensions;
using CustomerPortal.UserAuthService.Postgres.Extensions;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUserAuthServicePostgres(o =>
    builder.Configuration.GetSection("Postgres").Bind(o)
);
builder.Services.AddUserAuthService();

builder.Services.AddProblemDetails(o =>
    o.CustomizeProblemDetails = ctx =>
    {
        var statusCode = ctx.Exception switch
        {
            EntityNotFoundException => (int)HttpStatusCode.NotFound,
            OperationConflictException => (int)HttpStatusCode.Conflict,
            DomainValidationException => (int)HttpStatusCode.UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        };

        var title = ctx.Exception switch
        {
            EntityNotFoundException => "Entity not found.",
            OperationConflictException => "Conflict occurred.",
            DomainValidationException => "Domain logic validation error.",
            _ => "Internal server error.",
        };

        ctx.ProblemDetails.Title = title;
        ctx.HttpContext.Response.StatusCode = statusCode;
        ctx.ProblemDetails.Status = statusCode;
        ctx.ProblemDetails.Detail = ctx.Exception?.Message;
    }
);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddTransient<IPasswordHasher<string>, PasswordHasher<string>>();

var app = builder.Build();

await app.Services.InitializeUserAuthServicePostgres();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", [ExcludeFromDescription] () => Results.LocalRedirect("/scalar/v1"));

app.MapControllers();

await app.RunAsync();
