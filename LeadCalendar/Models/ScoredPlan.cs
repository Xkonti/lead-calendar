namespace LeadCalendar.Models;

public record ScoredPlan : FormedPlan
{
    public int Score { get; set; }
}