using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App7.Domain.Entities;

public class Model
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public int Available { get; set; } = 0;
    /// <summary>True when there is at least one device available to borrow.</summary>
    public bool IsAvailable => Available > 0;
}
