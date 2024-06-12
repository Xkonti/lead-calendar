namespace LeadCalendar.Models;

public struct Agent
{
    public string Name { get; init; }
    public bool[] ExcludedWeeks { get; init; }
}