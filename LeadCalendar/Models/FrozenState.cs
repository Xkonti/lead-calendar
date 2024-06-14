using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public record FrozenState(byte[] States, bool HasConflict, byte[] SelectionsPerWeek)
{
    public FrozenState(byte weeksCount) : this([], false, new byte[weeksCount]) {}
    
    public FrozenState AppendState(byte stateCombinationId, StateCombination state)
    {
        return new FrozenState(
            States.Append(stateCombinationId),
            HasConflict || state.HasConflict,
            SelectionsPerWeek.Increment(state.WeekSelections));
    }
}