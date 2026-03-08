namespace App7.Domain.Dtos;

public record GetModelsPagedRequest(
    int Page,
    int PageSize,
    string? SearchName = null,
    string? SearchManufacturer = null,
    string? FilterCategory = null,
    string? FilterSubCategory = null,
    string? SortColumn = null,
    bool Ascending = true
);
