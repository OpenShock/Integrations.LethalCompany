using System.Collections.Generic;

namespace OpenShock.Integrations.LethalCompany.OpenShockApi.Models;

public class ControlRequest
{
    public IEnumerable<Control> Shocks { get; set; }
    public string? CustomName { get; set; }
}