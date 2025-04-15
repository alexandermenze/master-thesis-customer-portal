using StackExchange.Redis;

namespace CustomerPortal.CatalogGenerationService;

public record StreamDatabase(IDatabase Database, string StreamName, string GroupName);
