using System.Collections.Generic;

namespace TaskMaster.Models;

public sealed class VerbConjugationResult
{
    public string InputWord { get; set; } = string.Empty;
    public string VerbType { get; set; } = string.Empty;
    public List<VerbConjugationItem> Forms { get; set; } = new();
}
