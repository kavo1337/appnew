using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class ConnectionType
{
    public int ConnectionTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Modem> Modem { get; set; } = new List<Modem>();
}
