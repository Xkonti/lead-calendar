using LeadCalendar.Models;

namespace LeadCalendar.Helpers;

public static class StateExtensions
{
    public static byte[] GetStateCombinationIds(this FrozenState state)
    {
        var result = new byte[state.StatesCount];
        var currentState = state;
        for (var i = 0; i < state.StatesCount; i++)
        {
            result[i] = currentState.LastStateCombinationId;
            currentState = currentState.ParentFrozenState;
        }

        result.Reverse();
        return result;
    }
}