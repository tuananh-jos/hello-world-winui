namespace App7.Domain.Dtos;

public record ModelFiltersResponse(
    IEnumerable<string> Manufacturers,
    IEnumerable<string> Categories,
    IEnumerable<string> SubCategories
);
