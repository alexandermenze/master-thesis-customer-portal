using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerPortal.CustomerWebsite.Pages;

public class RegisterModel(IHttpClientFactory httpClientFactory) : PageModel
{
    public class InputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("AuthApi");

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var response = await _httpClient.PostAsJsonAsync("users/register-customer", Input);

        if (response.IsSuccessStatusCode)
            return RedirectToPage("/Login");

        ModelState.AddModelError(string.Empty, "Registration failed.");
        return Page();
    }
}
