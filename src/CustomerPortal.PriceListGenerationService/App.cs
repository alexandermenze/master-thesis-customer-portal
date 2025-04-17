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
                streamDatabase.TaskStreamName,
                streamDatabase.ConsumerGroupName,
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

            await streamDatabase.Database.StreamAcknowledgeAsync(
                streamDatabase.TaskStreamName,
                streamDatabase.ConsumerGroupName,
                message.Id
            );
        }
    }

    private async Task SetupConsumer()
    {
        var streamExists = await streamDatabase.Database.KeyExistsAsync(
            streamDatabase.TaskStreamName
        );

        var consumerExists =
            streamExists
            && (
                await streamDatabase.Database.StreamGroupInfoAsync(streamDatabase.TaskStreamName)
            ).Any(g => g.Name.Equals(streamDatabase.ConsumerGroupName));

        if (consumerExists)
            return;

        await streamDatabase.Database.StreamCreateConsumerGroupAsync(
            streamDatabase.TaskStreamName,
            streamDatabase.ConsumerGroupName,
            "0-0"
        );
    }

    private async Task ProcessMessage(string body, CancellationToken ct)
    {
        var createCustomerPricelistCommand =
            JsonSerializer.Deserialize<CreateCustomerPriceListCommand>(body);

        if (createCustomerPricelistCommand is null)
            return;

        var pdfMemoryStream = PriceListPdfGenerator.GeneratePdf(
            createCustomerPricelistCommand.CustomerNo,
            createCustomerPricelistCommand.SalesOrg,
            createCustomerPricelistCommand.PriceDate
        );

        var dateTime = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        var fileName =
            $"PriceList_{Guid.CreateVersion7():N}_{dateTime}_{createCustomerPricelistCommand.SalesOrg}.pdf";
        var filePath = $"{createCustomerPricelistCommand.CustomerNo}/price-lists/{fileName}";

        await minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithStreamData(pdfMemoryStream)
                .WithObject(filePath)
                .WithObjectSize(pdfMemoryStream.Length)
                .WithBucket(minioBucket),
            ct
        );

        await streamDatabase.Database.StreamAddAsync(
            streamDatabase.ResponseStreamName,
            [
                new NameValueEntry("CreatedAt", DateTimeOffset.UtcNow.ToString("O")),
                new NameValueEntry(
                    "Body",
                    JsonSerializer.Serialize(
                        new CustomerPriceListGenerationFinishedEvent(
                            Guid.CreateVersion7(),
                            createCustomerPricelistCommand.Id,
                            createCustomerPricelistCommand.CustomerNo,
                            createCustomerPricelistCommand.SalesOrg,
                            createCustomerPricelistCommand.PriceDate,
                            filePath
                        )
                    )
                ),
            ]
        );
    }
}
