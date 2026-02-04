using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class VendingMachineManufacturer
{
    public int VendingMachineManufacturerId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<VendingMachineModel> VendingMachineModel { get; set; } = new List<VendingMachineModel>();
}
