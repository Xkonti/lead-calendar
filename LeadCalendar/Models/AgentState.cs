namespace LeadCalendar.Models;

public struct AgentState
{
    public int AgentId { get; init; }
    public bool[] SeletedWeeks { get; init; }
}