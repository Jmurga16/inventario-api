using Inventario.Application.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Inventario.Application.Services;

public class ReportService : IReportService
{
    private readonly IProductService _productService;

    public ReportService(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<byte[]> GenerateLowStockReportPdfAsync()
    {
        var lowStockProducts = await _productService.GetLowStockAsync();
        var productList = lowStockProducts.ToList();

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeContent(c, productList));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("Low Stock Report")
                .FontSize(20)
                .Bold()
                .FontColor(Colors.Blue.Darken2);

            column.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                .FontSize(10)
                .FontColor(Colors.Grey.Darken1);

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComposeContent(IContainer container, List<DTOs.Products.ProductDto> products)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Item().Text($"Total products with low stock: {products.Count}")
                .FontSize(12)
                .Bold();

            column.Item().PaddingTop(15);

            if (products.Count == 0)
            {
                column.Item().Text("No products with low stock at this time.")
                    .FontSize(12)
                    .Italic()
                    .FontColor(Colors.Grey.Darken1);
                return;
            }

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(50);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("ID").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("Name").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("SKU").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("Quantity").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("Min Stock").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("Unit Price").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                        .Text("Created Date").FontColor(Colors.White).Bold();
                });

                var isAlternate = false;
                foreach (var product in products)
                {
                    var bgColor = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                    string createdDate = product.CreatedAt.ToString("dd/MM/yyyy");

                    table.Cell().Background(bgColor).Padding(5).Text(product.Id.ToString());
                    table.Cell().Background(bgColor).Padding(5).Text(product.Name);
                    table.Cell().Background(bgColor).Padding(5).Text(product.SKU);
                    table.Cell().Background(bgColor).Padding(5)
                        .Text(product.Quantity.ToString())
                        .FontColor(product.Quantity == 0 ? Colors.Red.Darken1 : Colors.Orange.Darken2);
                    table.Cell().Background(bgColor).Padding(5).Text(product.MinStock.ToString());
                    table.Cell().Background(bgColor).Padding(5).Text($"${product.UnitPrice:N2}");

                    table.Cell().Background(bgColor).Padding(5).Text(createdDate);

                    isAlternate = !isAlternate;
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("Inventory Management System")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
                row.RelativeItem().AlignRight().Text(x =>
                {
                    x.Span("Page ").FontSize(9);
                    x.CurrentPageNumber().FontSize(9);
                    x.Span(" of ").FontSize(9);
                    x.TotalPages().FontSize(9);
                });
            });
        });
    }
}
