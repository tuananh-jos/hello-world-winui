namespace App7.Domain.Dtos;

public record ReturnDeviceRequest(
    Guid DeviceId,
    Guid ModelId
);
