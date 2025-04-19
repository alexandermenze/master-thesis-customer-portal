using CustomerPortal.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(
    "UserAuthService",
    o =>
        o.BaseAddress = new Uri(
            builder.Configuration.GetValueOrThrow<string>("UserAuthService:BaseUrl")
        )
);

builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Cookies";
        options.LoginPath = "/Login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration.GetValueOrThrow<string>("Redis:ConnectionString")
    )
);

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
