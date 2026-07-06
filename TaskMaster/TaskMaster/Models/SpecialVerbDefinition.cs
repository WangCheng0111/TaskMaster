using System.Collections.Generic;

namespace TaskMaster.Models;

public sealed class SpecialVerbDefinition
{
    public string Word { get; set; } = string.Empty;
    public VerbType VerbType { get; set; } = VerbType.Unknown;
    public Dictionary<string, string> Forms { get; set; } = new();
}
