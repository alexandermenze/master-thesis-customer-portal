namespace CustomerPortal.Messages.Events;

public record CustomerPriceListGenerationFinishedEvent(
    Guid Id,
    Guid CommandId,
    int CustomerNo,
    string SalesOrg,
    DateOnly PriceDate,
    string StorageFilePath
);
