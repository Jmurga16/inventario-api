namespace Inventario.Application.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateLowStockReportPdfAsync();
}
