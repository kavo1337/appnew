using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class VendingMachineIncome
{
    public int VendingMachineId { get; set; }

    public decimal? TotalIncome { get; set; }
}
