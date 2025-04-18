using StackExchange.Redis;

namespace CustomerPortal.PriceListGenerationService;

public record StreamDatabase(IDatabase Database, string TasksStreamName, string ConsumerGroupName);
