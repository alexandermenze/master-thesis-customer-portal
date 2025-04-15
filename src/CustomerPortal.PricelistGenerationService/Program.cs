using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var customerNo = 123456;
var priceDate = DateOnly.FromDateTime(DateTime.Today);

await Task.Delay(10000);
