using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class CriticalValuesTemplate
{
    public int CriticalValuesTemplateId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<VendingMachine> VendingMachine { get; set; } = new List<VendingMachine>();
}
