using System.Net.Http.Headers;
using CustomerPortal.Messages.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerPortal.CustomerWebsite.Pages;

public class Tasks(ILogger<Tasks> logger, IHttpClientFactory httpClientFactory) : PageModel
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
}
