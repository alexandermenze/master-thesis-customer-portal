using StackExchange.Redis;

namespace CustomerPortal.PriceListGenerationService;

public record StreamDatabase(
    IDatabase Database,
    string TaskStreamName,
    string ConsumerGroupName,
    string ResponseStreamName
);
