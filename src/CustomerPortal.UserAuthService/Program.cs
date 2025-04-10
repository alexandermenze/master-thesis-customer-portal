using CustomerPortal.UserAuthService.Postgres;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContextPool<UserAuthContext>(o =>
    o.UseNpgsql(builder.Configuration.GetValue<string>("Postgres:ConnectionString"))
);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapGet("/", [ExcludeFromDescription] () => Results.LocalRedirect("/scalar/v1"));

await app.RunAsync();
