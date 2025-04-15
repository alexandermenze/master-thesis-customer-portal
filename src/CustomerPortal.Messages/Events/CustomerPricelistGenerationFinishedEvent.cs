namespace CustomerPortal.Messages.Events;

public record CustomerPricelistGenerationFinishedEvent(
    int CustomerNo,
    string SalesOrg,
    DateTimeOffset PriceDate,
    string StorageFilePath
);
