using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class Country
{
    public int CountryId { get; set; }

    public string Name { get; set; } = null!;

    public string? IsoCode { get; set; }

    public virtual ICollection<VendingMachine> VendingMachine { get; set; } = new List<VendingMachine>();
}
