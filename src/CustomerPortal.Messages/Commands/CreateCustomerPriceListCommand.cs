namespace CustomerPortal.Messages.Commands;

public record CreateCustomerPriceListCommand(
    Guid Id,
    int CustomerNo,
    string SalesOrg,
    DateOnly PriceDate
);
