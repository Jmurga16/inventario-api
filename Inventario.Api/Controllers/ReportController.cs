using Inventario.Application.DTOs.Common;
using Inventario.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Generates a PDF report with products that have low stock
    /// </summary>
    [HttpGet("low-stock/pdf")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLowStockPdf()
    {
        var pdfBytes = await _reportService.GenerateLowStockReportPdfAsync();

        return File(
            pdfBytes,
            "application/pdf",
            $"low-stock-report-{DateTime.Now:yyyyMMdd-HHmmss}.pdf"
        );
    }

    /// <summary>
    /// Returns low stock report data as JSON (for preview before PDF)
    /// </summary>
    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> GetLowStockData(
        [FromServices] IProductService productService)
    {
        var products = await productService.GetLowStockAsync();
        var reportData = new
        {
            GeneratedAt = DateTime.Now,
            TotalProducts = products.Count(),
            Products = products
        };

        return Ok(ApiResponse<object>.Ok(reportData));
    }
}
