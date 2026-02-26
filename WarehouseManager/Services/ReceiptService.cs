using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WarehouseManager.Models;

namespace WarehouseManager.Services
{
    public static class ReceiptService
    {
        public static void GeneratePdfReceipt(IEnumerable<Product> purchasedItems, decimal totalAmount)
        {
            // A QuestPDF ingyenes közösségi licencének beállítása
            QuestPDF.Settings.License = LicenseType.Community;

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var filePath = Path.Combine(desktopPath, $"Nyugta_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            // A kosár elemeinek csoportosítása (ha valamiből többet vett)
            var groupedItems = purchasedItems.GroupBy(p => p.Name)
                .Select(g => new { Name = g.Key, Quantity = g.Count(), UnitPrice = g.First().Price, Total = g.Sum(x => x.Price) })
                .ToList();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    // Fejléc
                    page.Header().Column(col =>
                    {
                        col.Item().Text("WMS PRO - VÁSÁRLÁSI NYUGTA").SemiBold().FontSize(20).FontColor(Colors.Blue.Darken2);
                        col.Item().Text($"Dátum: {DateTime.Now:yyyy. MM. dd. HH:mm}");
                        col.Item().PaddingBottom(1, Unit.Centimetre).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                    });

                    // Tartalom (Táblázat)
                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Név
                            columns.RelativeColumn(1); // Mennyiség
                            columns.RelativeColumn(1); // Egységár
                            columns.RelativeColumn(1); // Összesen
                        });

                        // Táblázat fejléce
                        table.Header(header =>
                        {
                            header.Cell().Text("Termék").SemiBold();
                            header.Cell().AlignRight().Text("Db").SemiBold();
                            header.Cell().AlignRight().Text("Egységár").SemiBold();
                            header.Cell().AlignRight().Text("Összesen").SemiBold();
                        });

                        // Termékek listázása
                        foreach (var item in groupedItems)
                        {
                            table.Cell().Text(item.Name);
                            table.Cell().AlignRight().Text(item.Quantity.ToString());
                            table.Cell().AlignRight().Text($"{item.UnitPrice:N0} Ft");
                            table.Cell().AlignRight().Text($"{item.Total:N0} Ft");
                        }
                    });

                    // Lábjegyzet (Végösszeg)
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        col.Item().PaddingTop(0.5f, Unit.Centimetre).AlignRight()
                           .Text($"Fizetendő összesen: {totalAmount:N0} Ft").FontSize(16).SemiBold().FontColor(Colors.Green.Darken2);
                    });
                });
            })
            .GeneratePdf(filePath);
        }
    }
}