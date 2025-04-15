namespace CustomerPortal.Messages.Commands;

public record CreateCustomerPricelistCommand(int CustomerNo, string SalesOrg, DateOnly PriceDate);
