using System.Collections.Immutable;
using System.Net.Http.Headers;
using CustomerPortal.InternalWebsite.Models;
using CustomerPortal.Messages.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace CustomerPortal.InternalWebsite.Pages;

public class Users(ILogger<Users> logger, IHttpClientFactory httpClientFactory)
    : UserPageModel(logger, httpClientFactory)
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UserAuthService");

    public ImmutableArray<UserResponseDto> AllUsers { get; set; } = [];

    [InboundDataflow("sales-dept-website", "manage-user-accounts")]
    public async Task<IActionResult> OnGet()
    {
        var bearerToken = User.FindFirst("BearerToken")?.Value;

        if (bearerToken is null)
            return RedirectToPage("/Login");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            bearerToken
        );

        var all = await Pull(
            "list-users",
            () => _httpClient.GetFromJsonAsync<ImmutableArray<UserResponseDto>>("/users")
        );

        AllUsers = all;
        return Page();
    }

    [InboundDataflow("sales-dept-website", "manage-user-accounts")]
    public async Task<IActionResult> OnPostDeactivateAsync(Guid id)
    {
        var bearerToken = User.FindFirst("BearerToken")?.Value;

        if (bearerToken is null)
            return RedirectToPage("/Login");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            bearerToken
        );

        var response = await Push(
            "approve-user",
            () => _httpClient.PatchAsJsonAsync($"/users/{id}/deactivate", new { })
        );

        if (response.IsSuccessStatusCode)
            return RedirectToPage();

        ModelState.AddModelError(string.Empty, "Fehler beim Deaktivieren des Nutzers.");
        return await OnGet();
    }

    [InboundDataflow("sales-dept-website", "manage-user-accounts")]
    public async Task<IActionResult> OnPostApproveAsync(Guid id)
    {
        await OnGet();

        var user = AllUsers.FirstOrDefault(u => u.Id == id);

        if (user?.CustomerNo is null)
            return RedirectToPage();

        var response = await Push(
            "approve-user",
            () => _httpClient.PostAsJsonAsync($"/users/{id}/approve", new { user.CustomerNo })
        );

        if (response.IsSuccessStatusCode)
            return RedirectToPage();

        ModelState.AddModelError(string.Empty, "Fehler beim Deaktivieren des Nutzers.");
        return await OnGet();
    }
}
