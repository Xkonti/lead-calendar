namespace LeadCalendar.Models;

public struct StateCombination
{
    /// <summary>
    /// Array of bools representing the selections for each week. True means the week is selected.
    /// Week 0 is the last week of the previous period.
    /// At this point it doesn't matter which weeks are excluded, etc.
    /// </summary>
    public bool[] WeekSelections { get; init; }
    
    /// <summary>
    /// True if there is a conflict - at least two consecutive weeks selected.
    /// </summary>
    public bool HasConflict { get; init; }

    public override string ToString()
    {
        return string.Join("", WeekSelections.Select(x => x ? "X" : "_")) + " " + (HasConflict ? "CONF" : "");
    }
}