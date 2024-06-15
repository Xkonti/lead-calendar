using System.Text;
using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public sealed record FrozenState(
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
    
    

    public bool[][] GetStatePerAgent(AgentStateCombinations[] combinationsPerAgent)
    {
        var agentCount = combinationsPerAgent.Length;
        var stateIds = this.GetStateCombinationIds();
        if (agentCount != stateIds.Length) throw new ArgumentException("State and combinationsPerAgent must have the same length");
        var result = new bool[agentCount][];
        for (var agentId = 0; agentId < agentCount; agentId++)
        {
            result[agentId] = combinationsPerAgent[agentId].Combinations[stateIds[agentId]].WeekSelections;
        }
        return result;
    }

    public string PresentAsList(string[] agentNames, AgentStateCombinations[] combinationsPerAgent)
    {
        var sb = new StringBuilder();
        var states = this.GetStatePerAgent(combinationsPerAgent);
        for (var weekId = 1; weekId <= SelectionsPerWeek.Length; weekId++)
        {
            sb.Append($"Week {weekId}: ");
            var assignedNames = new List<string>();
            for (var agentId = 0; agentId < agentNames.Length; agentId++)
            {
                if (states[agentId][weekId]) assignedNames.Add(agentNames[agentId]);
            }
            sb.Append(string.Join(", ", assignedNames));
            sb.AppendLine();
        }
        return sb.ToString();
    }
}