using System;
using System.Collections.Generic;

namespace app.API.Data.Entities;

public partial class EventType
{
    public int EventTypeId { get; set; }

    public int EventSeverityId { get; set; }

    public string Name { get; set; } = null!;

    public virtual EventSeverity EventSeverity { get; set; } = null!;

    public virtual ICollection<VendingMachineEvent> VendingMachineEvent { get; set; } = new List<VendingMachineEvent>();
}
