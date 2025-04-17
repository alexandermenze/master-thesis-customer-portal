namespace CustomerPortal.Messages.Events;

public record CustomerPriceListGenerationFinishedEvent(
    int CustomerNo,
    string SalesOrg,
    DateTimeOffset PriceDate,
    string StorageFilePath
);
