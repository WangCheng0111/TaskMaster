namespace TaskMaster.Models;

public sealed class VerbConjugationItem
{
    public string FormName { get; set; } = string.Empty;
    public string FormValue { get; set; } = "无";
    public bool IsAvailable { get; set; }
    public string AvailabilityText => IsAvailable ? "可用" : "无";
}
