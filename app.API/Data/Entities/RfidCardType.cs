using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class RfidCardType
{
    public int RfidCardTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<VendingMachineRfidCard> VendingMachineRfidCard { get; set; } = new List<VendingMachineRfidCard>();
}
