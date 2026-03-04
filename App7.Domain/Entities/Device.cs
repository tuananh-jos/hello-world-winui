using System.ComponentModel.DataAnnotations.Schema;

namespace App7.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public Guid ModelId { get; set; }

    public string IMEI { get; set; } = string.Empty;
    public string SerialLab { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string CircuitSerialNumber { get; set; } = string.Empty;
    public string HWVersion { get; set; } = string.Empty;
    public string Status { get; set; } = "Available";

    /// <summary>
    /// Populated by join at query time — never stored in DB.
    /// </summary>
    [NotMapped]
    public string ModelName { get; set; } = string.Empty;
}
