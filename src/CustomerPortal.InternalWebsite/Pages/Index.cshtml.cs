using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CustomerPortal.InternalWebsite.Pages;

public class Index : PageModel
{
    [ThreatModelProcess("sales-dept-website")]
    public IActionResult OnGet() => RedirectToPage("/UnapprovedUsers");
}
