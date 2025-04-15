using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var customerNo = 123456;
var priceDate = DateOnly.FromDateTime(DateTime.Today);

Document
    .Create(document =>
        document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);

            page.Header().Text("Products Pricelist").FontSize(18).Bold();

            page.Content()
                .PaddingVertical(0.5f, Unit.Centimetre)
                .Column(column =>
                {
                    column
                        .Item()
                        .Height(5, Unit.Centimetre)
                        .Row(row =>
                        {
                            row.RelativeItem(0.7f)
                                .Column(innerColumn =>
                                {
                                    innerColumn.Item().Text($"Customer No: {customerNo}");
                                    innerColumn.Item().Text($"Name: {Placeholders.Name()}");
                                    innerColumn
                                        .Item()
                                        .Text($"Price date: {priceDate.ToString("O")}");
                                });

                            row.RelativeItem(0.3f).Image(Placeholders.Image);
                        });

                    column.Item().Height(2, Unit.Centimetre);

                    column
                        .Item()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.ConstantColumn(125);
                            });

                            table.Header(header =>
                            {
                                header.Cell().BorderBottom(2).Padding(8).Text("#");
                                header.Cell().BorderBottom(2).Padding(8).Text("Product");
                                header.Cell().BorderBottom(2).Padding(8).AlignRight().Text("Price");
                            });

                            foreach (var i in Enumerable.Range(0, 6))
                            {
                                var price = Math.Round(Random.Shared.NextDouble() * 100, 2);

                                table.Cell().Padding(8).Text($"{i + 1}");
                                table.Cell().Padding(8).Text(Placeholders.Label());
                                table.Cell().Padding(8).AlignRight().Text($"${price}");
                            }
                        });
                });

            page.Footer().Text("Company Inc.").FontSize(12).Bold();
        })
    )
    .GeneratePdfAndShow();

await Task.Delay(10000);
