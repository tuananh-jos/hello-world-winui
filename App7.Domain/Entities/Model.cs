using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App7.Domain.Entities;
public class Model
{
    public Guid Id
    {
        get; set;
    }
    public string Name
    {
        get; set;
    }
    public string Manufacturer
    {
        get; set;
    }
    public string Category
    {
        get; set;
    }
    public string SubCategory
    {
        get; set;
    }
    public int Available
    {
        get; set;
    }

    /// <summary>True when there is at least one device available to borrow.</summary>
    public bool IsAvailable => Available > 0;
}
