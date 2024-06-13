using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public record FrozenState(int[] States, bool HasConflict, int[] SelectionsPerWeek)
{
    public FrozenState AppendState(int stateId, StateCombination state)
    {
        return new FrozenState(
            States.Append(stateId),
            HasConflict || state.HasConflict,
            SelectionsPerWeek.Increment(state.WeekSelections));
    }
}