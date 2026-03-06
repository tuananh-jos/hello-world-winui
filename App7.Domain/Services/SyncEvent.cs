namespace App7.Domain.Services;

/// <summary>
/// Represents a single borrow or return event appended to signal.txt.
/// Each line in signal.txt is a JSON-serialized SyncEvent.
/// Format: {"Seq":1,"Action":"borrow","ModelId":"...","DeviceIds":["..."],"NewAvailableCount":8}
/// </summary>
public class SyncEvent
{
    public long   Seq              { get; set; }
    public string Action           { get; set; } = string.Empty; // "borrow" | "return"
    public Guid   ModelId          { get; set; }
    public List<Guid> DeviceIds    { get; set; } = new();
    public int    NewAvailableCount { get; set; }
}
