using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// TODO: Setup postgres connection

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", [ExcludeFromDescription] () => Results.LocalRedirect("/scalar/v1"));

await app.RunAsync();
