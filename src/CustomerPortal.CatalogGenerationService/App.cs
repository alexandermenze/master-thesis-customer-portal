using System.Text.Json;
using CustomerPortal.Messages.Commands;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using StackExchange.Redis;

namespace CustomerPortal.CatalogGenerationService;

public class App(StreamDatabase streamDatabase)
{
    public async Task Run(CancellationToken ct)
    {
        await SetupConsumer();

        await streamDatabase.Database.StreamAddAsync(
            streamDatabase.StreamName,
            [
                new NameValueEntry("CreatedAt", DateTimeOffset.UtcNow.ToString("O")),
                new NameValueEntry(
                    "Body",
                    JsonSerializer.Serialize(
                        new CreateCustomerPricelistCommand(
                            123,
                            "0080",
                            DateOnly.FromDateTime(DateTime.Today)
                        )
                    )
                ),
            ]
        );

        while (ct.IsCancellationRequested is false)
        {
            var result = await streamDatabase.Database.StreamReadGroupAsync(
                streamDatabase.StreamName,
                streamDatabase.GroupName,
                consumerName: "catalog-generation-service-1",
                position: ">",
                count: 1
            );

            if (result.Length is not 1)
            {
                await Task.Delay(1000, ct);
                continue;
            }

            var message = result[0];
            var body = message["Body"];
            await ProcessMessage(body.ToString(), ct);
        }
    }

    private async Task SetupConsumer()
    {
        if (await streamDatabase.Database.KeyExistsAsync(streamDatabase.StreamName))
            return;

        var groupNames = await streamDatabase.Database.StreamGroupInfoAsync(
            streamDatabase.StreamName
        );

        if (groupNames.Any(g => g.Name.Equals(streamDatabase.GroupName)))
            return;

        await streamDatabase.Database.StreamCreateConsumerGroupAsync(
            streamDatabase.StreamName,
            streamDatabase.GroupName,
            "0-0"
        );
    }

    private async Task ProcessMessage(string body, CancellationToken ct)
    {
        var createCustomerPricelistCommand =
            JsonSerializer.Deserialize<CreateCustomerPricelistCommand>(body);

        Document
            .Create(document =>
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);

                    page.Header().Text("Products Pricelist").FontSize(18).Bold();

                    page.Content().PaddingVertical(0.5f, Unit.Centimetre).Column(column =>
                    {
                        column.Item().Text(Placeholders.)
                    });

                    page.Footer().Text("Company Inc.").FontSize(12).Bold();
                })
            )
            .GeneratePdfAndShow();
    }
}
