using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class VendingMachineEquipment
{
    public int VendingMachineId { get; set; }

    public int EquipmentTypeId { get; set; }

    public bool IsOperational { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual EquipmentType EquipmentType { get; set; } = null!;

    public virtual VendingMachine VendingMachine { get; set; } = null!;
}
