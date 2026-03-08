namespace App7.Domain.Dtos;

public record GetBorrowedDevicesRequest(
    int Page,
    int PageSize,
    string? SearchName = null,
    string? SearchModelName = null,
    string? SearchIMEI = null,
    string? SearchSerialLab = null,
    string? SearchSerialNumber = null,
    string? SearchCircuitSerial = null,
    string? SearchHWVersion = null,
    string? SortColumn = null,
    bool Ascending = true
);