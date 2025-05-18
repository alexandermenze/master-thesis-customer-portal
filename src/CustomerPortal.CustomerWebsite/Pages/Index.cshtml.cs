using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerPortal.CustomerWebsite.Pages;

public class Index : PageModel
{
    [ThreatModelProcess("customer-website-core")]
    public IActionResult OnGet() => RedirectToPage("/PriceLists");
}
