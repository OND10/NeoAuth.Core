using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs;

public record CreateProductRequest(
    [Required] string Name,
    [Range(0.01, 1000000)] decimal Price
);

public record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    DateTime CreatedAt
);
