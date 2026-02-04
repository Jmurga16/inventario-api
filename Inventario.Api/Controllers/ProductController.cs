using Inventario.Application.DTOs.Common;
using Inventario.Application.DTOs.Products;
using Inventario.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll()
    {
        var products = await _productService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetById(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product == null)
            return NotFound(ApiResponse.Fail("Product not found"));

        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> Search([FromQuery] ProductFilterDto filter)
    {
        var products = await _productService.SearchAsync(filter);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetLowStock()
    {
        var products = await _productService.GetLowStockAsync();
        return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create([FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ApiResponse<ProductDto>.Ok(product, "Product created"));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated"));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse>> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Product deleted"));
    }
}
