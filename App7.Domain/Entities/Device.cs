namespace App7.Domain.Entities;

public class Device
{
    public Guid Id
    {
        get; set;
    }
    public Guid ModelId
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public string IMEI
    {
        get; set;
    }
    public string SerialLab
    {
        get; set;
    }
    public string SerialNumber
    {
        get; set;
    }
    public string CircuitSerialNumber
    {
        get; set;
    }
    public string HWVersion
    {
        get; set;
    }
    public string Status
    {
        get; set;
    } = "Available";
}

