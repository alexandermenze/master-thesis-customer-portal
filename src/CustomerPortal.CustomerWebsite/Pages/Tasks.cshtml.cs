using System.Collections.Immutable;
using System.Net.Http.Headers;
using CustomerPortal.Messages.Dtos;
using CustomerPortal.Messages.Events;
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

        var state = new Dictionary<Guid, TaskStatus>();

        foreach (var entry in entries)
        {
            if (entry["CustomerNo"] != customerNo)
                continue;

            var taskId = Guid.TryParse(entry["TaskId"], out var id) ? (Guid?)id : null;

            if (taskId is null)
                continue;

            var entryType = entry["Type"];

            if (entryType.HasValue is false)
                continue;

            var taskStatusChange = entryType.ToString() switch
            {
                nameof(CustomerPriceListGenerationStartedEvent) =>
                    MapFromCustomerPriceListGenerationStartedEvent(entry),
                nameof(CustomerPriceListGeneratedEvent) => MapFromCustomerPriceListGeneratedEvent(
                    entry
                ),
                _ => null,
            };

            var taskState =
                state.GetValueOrDefault(taskId.Value) ?? new TaskStatus(taskId.Value, [], null);

            taskState = taskState with { History = taskState.History.Add("") };
        }
    }

    private TaskStatusChange MapFromCustomerPriceListGenerationStartedEvent(StreamEntry entry)
    {
        throw new NotImplementedException();
    }

    private TaskStatusChange MapFromCustomerPriceListGeneratedEvent(StreamEntry generatedEvent)
    {
        throw new NotImplementedException();
    }

    private record TaskStatus(Guid Id, ImmutableArray<TaskStatusChange> History);

    private record TaskStatusChange(
        DateTimeOffset DateTime,
        string Description,
        string? FileDownloadLink
    );
}
