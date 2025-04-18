using System.Text.Json;
using CustomerPortal.Messages.Commands;
using CustomerPortal.Messages.Events;
using Minio;
using Minio.DataModel.Args;
using StackExchange.Redis;

namespace CustomerPortal.PriceListGenerationService;

public class App(StreamDatabase streamDatabase, IMinioClient minioClient, string minioBucket)
{
    public async Task Run(CancellationToken ct)
    {
        await SetupConsumer();

        while (ct.IsCancellationRequested is false)
        {
            var result = await streamDatabase.Database.StreamReadGroupAsync(
                streamDatabase.TasksStreamName,
                streamDatabase.ConsumerGroupName,
                consumerName: "price-list-generation-service",
                position: ">",
                count: 1
            );

            if (result.Length is not 1)
            {
                await Task.Delay(1000, ct);
                continue;
            }

            var message = result[0];

            if (message["Type"] == nameof(GenerateCustomerPriceListCommand))
            {
                var body = message["Body"];
                await ProcessMessage(body.ToString(), ct);
            }

            await streamDatabase.Database.StreamAcknowledgeAsync(
                streamDatabase.TasksStreamName,
                streamDatabase.ConsumerGroupName,
                message.Id
            );
        }
    }

    private async Task SetupConsumer()
    {
        var streamExists = await streamDatabase.Database.KeyExistsAsync(
            streamDatabase.TasksStreamName
        );

        var consumerExists =
            streamExists
            && (
                await streamDatabase.Database.StreamGroupInfoAsync(streamDatabase.TasksStreamName)
            ).Any(g => g.Name.Equals(streamDatabase.ConsumerGroupName));

        if (consumerExists)
            return;

        await streamDatabase.Database.StreamCreateConsumerGroupAsync(
            streamDatabase.TasksStreamName,
            streamDatabase.ConsumerGroupName,
            "0-0"
        );
    }

    private async Task ProcessMessage(string body, CancellationToken ct)
    {
        var generateCustomerPriceListCommand =
            JsonSerializer.Deserialize<GenerateCustomerPriceListCommand>(body);

        if (generateCustomerPriceListCommand is null)
            return;

        await streamDatabase.Database.StreamAddAsync(
            streamDatabase.TasksStreamName,
            [
                new NameValueEntry("Type", nameof(CustomerPriceListGenerationStartedEvent)),
                new NameValueEntry("UserId", generateCustomerPriceListCommand.UserId.ToString()),
                new NameValueEntry("CustomerNo", generateCustomerPriceListCommand.CustomerNo),
                new NameValueEntry("CreatedAt", DateTimeOffset.UtcNow.ToString("O")),
                new NameValueEntry(
                    "Body",
                    JsonSerializer.Serialize(
                        new CustomerPriceListGenerationStartedEvent(
                            Guid.CreateVersion7(),
                            generateCustomerPriceListCommand.Id,
                            generateCustomerPriceListCommand.SalesOrg,
                            generateCustomerPriceListCommand.PriceDate
                        )
                    )
                ),
            ]
        );

        var pdfMemoryStream = PriceListPdfGenerator.GeneratePdf(
            generateCustomerPriceListCommand.CustomerNo,
            generateCustomerPriceListCommand.SalesOrg,
            generateCustomerPriceListCommand.PriceDate
        );

        var dateTime = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName =
            $"PriceList_{Guid.CreateVersion7():N}_{dateTime}_{generateCustomerPriceListCommand.SalesOrg}.pdf";
        var filePath = $"{generateCustomerPriceListCommand.CustomerNo}/price-lists/{fileName}";

        await minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithStreamData(pdfMemoryStream)
                .WithObject(filePath)
                .WithObjectSize(pdfMemoryStream.Length)
                .WithBucket(minioBucket),
            ct
        );

        await streamDatabase.Database.StreamAddAsync(
            streamDatabase.TasksStreamName,
            [
                new NameValueEntry("Type", nameof(CustomerPriceListGeneratedEvent)),
                new NameValueEntry("UserId", generateCustomerPriceListCommand.UserId.ToString()),
                new NameValueEntry("CustomerNo", generateCustomerPriceListCommand.CustomerNo),
                new NameValueEntry("CreatedAt", DateTimeOffset.UtcNow.ToString("O")),
                new NameValueEntry(
                    "Body",
                    JsonSerializer.Serialize(
                        new CustomerPriceListGeneratedEvent(
                            Guid.CreateVersion7(),
                            generateCustomerPriceListCommand.Id,
                            generateCustomerPriceListCommand.SalesOrg,
                            generateCustomerPriceListCommand.PriceDate,
                            filePath
                        )
                    )
                ),
            ]
        );
    }
}
