namespace App7.Domain.Dtos;

public record BorrowDeviceRequest(
    Guid ModelId,
    int Quantity
);
