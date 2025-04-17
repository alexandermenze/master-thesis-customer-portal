using StackExchange.Redis;

namespace CustomerPortal.PriceListGenerationService;

public record StreamDatabase(IDatabase Database, string StreamName, string GroupName);
