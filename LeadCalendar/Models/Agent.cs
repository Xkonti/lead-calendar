namespace LeadCalendar.Models;

public struct Agent
{
    public string Name { get; init; }
    public int[] ExcludedWeeks { get; init; }
    
    public Agent(string name, params int[] excludedWeeks)
    {
        Name = name;
        ExcludedWeeks = excludedWeeks;
    }
}