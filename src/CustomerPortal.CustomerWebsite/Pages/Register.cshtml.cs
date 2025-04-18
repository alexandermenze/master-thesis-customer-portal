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

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UserAuthService");

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var response = await _httpClient.PostAsJsonAsync("users/register-customer", Input);

        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] =
                "Ihr Account wird nun manuell geprüft und freigeschaltet. Sie erhalten eine Benachrichtigung per E-Mail.";
        }
        else
        {
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            var errorMessage = problemDetails?.Detail ?? "Es ist ein Fehler aufgetreten.";

            ModelState.AddModelError(string.Empty, errorMessage);
        }

        return Page();
    }
}
