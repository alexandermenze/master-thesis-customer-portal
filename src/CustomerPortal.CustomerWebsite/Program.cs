using CustomerPortal.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(
    "UserAuthService",
    o =>
        o.BaseAddress = new Uri(
            builder.Configuration.GetValueOrThrow<string>("UserAuthService:BaseUrl")
        )
);

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();
