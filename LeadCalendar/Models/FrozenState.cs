using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public record FrozenState(int[] States, bool HasConflict, int[] SelectionsPerWeek)
{
    public FrozenState(int weeksCount) : this([], false, new int[weeksCount]) {}
    
    public FrozenState AppendState(int stateCombinationId, StateCombination state)
    {
        return new FrozenState(
            States.Append(stateCombinationId),
            HasConflict || state.HasConflict,
            SelectionsPerWeek.Increment(state.WeekSelections));
    }
}