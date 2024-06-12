namespace LeadCalendar.Models;

public record PlanInProgress : FormedPlan
{
    public int LockedStates { get; set; }
}