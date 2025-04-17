namespace CustomerPortal.Messages.Commands;

public record CreateCustomerPriceListCommand(int CustomerNo, string SalesOrg, DateOnly PriceDate);
