using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public record FrozenState(
    FrozenState? ParentFrozenState,
    byte LastStateCombinationId,
    byte StatesCount,
    bool HasConflict,
    byte[] SelectionsPerWeek)
{
    
    
    public FrozenState(byte weeksCount) : this(null, 0, 0, false, new byte[weeksCount]) {}
    
    public FrozenState AppendState(byte stateCombinationId, StateCombination state)
    {
        return new FrozenState(
            this,
            stateCombinationId,
            (byte)(StatesCount + 1),
            HasConflict || state.HasConflict,
            SelectionsPerWeek.Increment(state.WeekSelections));
    }
}