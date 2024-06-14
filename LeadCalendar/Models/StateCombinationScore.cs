namespace LeadCalendar.Models;

public struct StateCombinationScore
{
    public int ConflictScore { get; init; }

    public StateCombinationScore(bool[] weekSelections)
    {
        // TODO: Calculate score
        ConflictScore = 0;
    }
}