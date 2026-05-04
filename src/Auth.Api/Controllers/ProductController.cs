using Auth.Api.Authorization;
using Auth.Application.DTOs;
using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication for all endpoints
public class ProductController : ControllerBase
{
    private readonly AuthDbContext _context;

    public ProductController(AuthDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("Products.Read")]
    [RequireScope("products.read")] // Allows either user with permission OR client with scope
    public async Task<ActionResult<IEnumerable<ProductResponse>>> GetProducts()
    {
        var products = await _context.Products
            .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.CreatedAt))
            .ToListAsync();
        
        return Ok(Result.Success(products));
    }

    [HttpPost]
    [RequirePermission("Products.Create")]
    [RequireScope("products.write")]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, 
            Result.Success(new ProductResponse(product.Id, product.Name, product.Price, product.CreatedAt)));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Products.Delete")]
    [RequireScope("products.delete")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(Result.Success(new { Message = "Product deleted." }));
    }
}
