using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text.Json;
using CustomerPortal.Messages.Dtos;
using CustomerPortal.Messages.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Minio;
using Minio.DataModel.Args;
using StackExchange.Redis;

namespace CustomerPortal.CustomerWebsite.Pages;

public class Tasks(
    ILogger<Tasks> logger,
    IHttpClientFactory httpClientFactory,
    IConnectionMultiplexer redis,
    IMinioClient minio
) : PageModel
{
    public class TaskViewModel
    {
        public Guid Id { get; set; }
        public DateTimeOffset FirstCreatedAt { get; set; }
        public string? LastDescription { get; set; }
        public bool HasDownload { get; set; }
    }

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("UserAuthService");

    public ImmutableArray<TaskViewModel> TaskViewModels { get; set; } =
        ImmutableArray<TaskViewModel>.Empty;

    public async Task<IActionResult> OnGet()
    {
        var token = User.FindFirst("BearerToken")?.Value;

        if (token is null)
            return RedirectToPage("/Login");

        var currentUser = await GetCurrentUser(token);

        if (currentUser?.CustomerNo is null)
            return RedirectToPage("/Login");

        ViewData["CurrentUser"] = currentUser;

        var userTasks = await GetUserTasks(currentUser.CustomerNo.Value);

        TaskViewModels =
        [
            .. userTasks
                .Values.Select(ts =>
                {
                    var first = ts.History.OrderBy(h => h.DateTime).First().DateTime;

                    var lastDesc = ts
                        .History.OrderByDescending(h => h.DateTime)
                        .First()
                        .Description;

                    var latestLink = ts
                        .History.Where(h => !string.IsNullOrEmpty(h.FileDownloadLink))
                        .OrderByDescending(h => h.DateTime)
                        .Select(h => h.FileDownloadLink)
                        .FirstOrDefault();

                    return new TaskViewModel
                    {
                        Id = ts.Id,
                        FirstCreatedAt = first,
                        LastDescription = lastDesc,
                        HasDownload = latestLink is not null,
                    };
                })
                .OrderByDescending(vm => vm.FirstCreatedAt),
        ];

        return Page();
    }

    public async Task<IActionResult> OnPostDownload(Guid id)
    {
        var token = User.FindFirst("BearerToken")?.Value;

        if (token is null)
            return RedirectToPage("/Login");

        var currentUser = await GetCurrentUser(token);

        if (currentUser?.CustomerNo is null)
            return RedirectToPage("/Login");

        var userTasks = await GetUserTasks(currentUser.CustomerNo.Value);

        var filePath = userTasks[id]
            .History.OrderByDescending(t => t.DateTime)
            .Last(t => t.FileDownloadLink is not null)
            .FileDownloadLink;

        const string bucketName = "customer-files";

        var memoryStream = new MemoryStream();

        await minio.GetObjectAsync(
            new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(filePath)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream))
        );

        var fileName = Path.GetFileName(filePath);

        memoryStream.Position = 0;
        const string contentType = "application/pdf";
        return File(memoryStream, contentType, fileName);
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

    private async Task<Dictionary<Guid, TaskStatus>> GetUserTasks(int customerNo)
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

            if (taskStatusChange is null)
                continue;

            var taskState =
                state.GetValueOrDefault(taskId.Value) ?? new TaskStatus(taskId.Value, []);

            taskState = taskState with { History = taskState.History.Add(taskStatusChange) };

            state[taskId.Value] = taskState;
        }

        return state;
    }

    private static TaskStatusChange MapFromCustomerPriceListGenerationStartedEvent(
        StreamEntry entry
    )
    {
        var createdAt = DateTimeOffset.Parse(entry["CreatedAt"].ToString());

        var body =
            JsonSerializer.Deserialize<CustomerPriceListGenerationStartedEvent>(
                entry["Body"].ToString()
            ) ?? throw new InvalidOperationException("Invalid body.");

        var description =
            $"Price list generation started for Sales Org {body.SalesOrg} with Price Date {body.PriceDate}.";

        return new TaskStatusChange(createdAt, description, null);
    }

    private static TaskStatusChange MapFromCustomerPriceListGeneratedEvent(StreamEntry entry)
    {
        var createdAt = DateTimeOffset.Parse(entry["CreatedAt"].ToString());

        var body =
            JsonSerializer.Deserialize<CustomerPriceListGeneratedEvent>(entry["Body"].ToString())
            ?? throw new InvalidOperationException("Invalid body.");

        var description =
            $"Price list generated for Sales Org {body.SalesOrg} with Price Date {body.PriceDate}.";

        return new TaskStatusChange(createdAt, description, body.StorageFilePath);
    }

    private record TaskStatus(Guid Id, ImmutableArray<TaskStatusChange> History);

    private record TaskStatusChange(
        DateTimeOffset DateTime,
        string Description,
        string? FileDownloadLink
    );
}
