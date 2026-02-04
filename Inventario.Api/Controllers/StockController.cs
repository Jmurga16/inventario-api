using Inventario.Application.DTOs.Common;
using Inventario.Application.DTOs.Stock;
using Inventario.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _stockService;

    public StockController(IStockService stockService)
    {
        _stockService = stockService;
    }

    [HttpPost("movement")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<StockMovementDto>>> AddMovement([FromBody] CreateStockMovementDto dto)
    {
        var movement = await _stockService.AddMovementAsync(dto);
        return Ok(ApiResponse<StockMovementDto>.Ok(movement, "Stock movement registered"));
    }

    [HttpGet("product/{productId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockMovementDto>>>> GetByProduct(int productId)
    {
        var movements = await _stockService.GetByProductIdAsync(productId);
        return Ok(ApiResponse<IEnumerable<StockMovementDto>>.Ok(movements));
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockMovementDto>>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var movements = await _stockService.GetByDateRangeAsync(startDate, endDate);
        return Ok(ApiResponse<IEnumerable<StockMovementDto>>.Ok(movements));
    }
}
