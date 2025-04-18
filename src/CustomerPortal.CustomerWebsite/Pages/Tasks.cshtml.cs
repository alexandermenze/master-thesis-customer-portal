using System.Net.Http.Headers;
using CustomerPortal.Messages.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StackExchange.Redis;

namespace CustomerPortal.CustomerWebsite.Pages;

public class Tasks(
    ILogger<Tasks> logger,
    IHttpClientFactory httpClientFactory,
    IConnectionMultiplexer redis
) : PageModel
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UserAuthService");

    public async Task<IActionResult> OnGet()
    {
        var token = User.FindFirst("BearerToken")?.Value;

        if (token is null)
            return RedirectToPage("/Login");

        var currentUser = await GetCurrentUser(token);

        if (currentUser is null)
            return RedirectToPage("/Login");

        ViewData["CurrentUser"] = currentUser;

        return Page();
    }

    private async Task<UserResponseDto?> GetCurrentUser(string token)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token
            );

            return await _httpClient.GetFromJsonAsync<UserResponseDto>("users/me");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get user");
            return null;
        }
    }

    private async Task GetUserTasks(int customerNo)
    {
        var entries = await redis.GetDatabase().StreamReadAsync("tasks", "0-0");

        // TODO: Somehow filter the tasks here and try to create the correct task state from it's events
    }
}
