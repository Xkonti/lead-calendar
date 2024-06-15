using LeadCalendar.Helpers;
using LeadCalendar.Models;

namespace LeadCalendar;

public class PlannerBuilder
{
    private readonly byte _weeksCount;
    private readonly byte _weeksPerAgent;
    private readonly byte _minAgentsPerWeek;
    private List<string> _agentNames = [];
    private List<byte[]> _excludedWeeksPerAgent = [];
    private List<byte> _previousWeekAgentIds = [];
    
    public PlannerBuilder(byte weeksCount, byte weeksPerAgent, byte minAgentsPerWeek)
    {
        _weeksCount = weeksCount;
        _weeksPerAgent = weeksPerAgent;
        _minAgentsPerWeek = minAgentsPerWeek;
    }

    public PlannerBuilder AddAgent(string name, params byte[] excludedWeeks)
    {
        _agentNames.Add(name);
        _excludedWeeksPerAgent.Add(excludedWeeks);
        return this;
    }
    
    public PlannerBuilder SetPreviousWeek(params string[] agentNames)
    {
        foreach (var agentName in agentNames)
        {
            var agentId = _agentNames.FindIndex(a => a == agentName);
            if (agentId == -1)
            {
                throw new Exception($"Agent with name {agentName} not found");
            }
            
            _previousWeekAgentIds.Add((byte)agentId);
        }
           
        return this;
    }

    public Planner Build()
    {

        var combinationsPerAgent = new List<StateCombination[]>();
        var firstConflictIndexes = new List<byte>();

        for (byte agentId = 0; agentId < _agentNames.Count; agentId++)
        {
            var (stateCombinations, firstConflictId) = GenerateStateCombinations(agentId);
            combinationsPerAgent.Add(stateCombinations);
            firstConflictIndexes.Add(firstConflictId);
        }
        
        var agentStateCombinations = combinationsPerAgent.Select(c => new AgentStateCombinations { Combinations = c.ToArray() }).ToArray();

        return new Planner(
            _weeksCount, _weeksPerAgent, _minAgentsPerWeek,
            _agentNames.ToArray(),
            agentStateCombinations,
            firstConflictIndexes.ToArray());
    }

    private byte[] GetEmptyWeeks(byte agentId)
    {
        var excludedWeeks = _excludedWeeksPerAgent[agentId];
        var emptyWeeks = new List<byte>();
        for (byte weekIndex = 1; weekIndex <= _weeksCount; weekIndex++)
        {
            if (excludedWeeks.Contains(weekIndex)) continue;
            emptyWeeks.Add(weekIndex);
        }
        return emptyWeeks.ToArray();
    }

    private (StateCombination[], byte) GenerateStateCombinations(byte agentId)
    {
        var emptyWeeks = GetEmptyWeeks(agentId);
        var combinations = CombinationsHelper.GenerateCombinations(emptyWeeks, _weeksPerAgent).ToArray();
        var stateCombinations = new List<StateCombination>();
        var initialState = CalculateInitialState(agentId);
        byte firstConflictIndex = 0;
        foreach (var combination in combinations)
        {
            var stateCombination = ApplyCombination(initialState, combination.Weeks);
            
            // Valid combinations are first, conflicting combinations are last
            // This way we can easily skip conflicting combinations in the future
            if (stateCombination.HasConflict)
            {
                stateCombinations.Add(stateCombination);
            }
            else
            {
                stateCombinations.Insert(0, stateCombination);
                firstConflictIndex++;
            }
        }
        
        return (stateCombinations.ToArray(), firstConflictIndex);
    }

    /// <summary>
    /// Calculates the initial state for the given agent.
    /// The initial state is an array of booleans, where each boolean represents a week.
    /// The boolean is true if the week is already booked (selected), and false if it's empty or excluded.
    /// </summary>
    private bool[] CalculateInitialState(byte agentId)
    {
        var isInPreviousWeek = _previousWeekAgentIds.Contains(agentId);
        var state = new bool[_weeksCount + 1];
        state[0] = isInPreviousWeek;
        return state;
    }

    private StateCombination ApplyCombination(bool[] initialState, byte[] combination)
    {
        var state = (bool[])initialState.Clone();
        foreach (var combinationWeek in combination)
        {
            state[combinationWeek] = true;
        }

        var hasConflict = DetectConflict(state);
        return new StateCombination { WeekSelections = state, HasConflict = hasConflict };
    }

    private bool DetectConflict(bool[] state)
    {
        // Check each week if it has conflict with previous week
        var previousWeek = state[0];
        for (byte week = 1; week <= _weeksCount; week++)
        {
            if (state[week] && previousWeek)
            {
                return true;
            }
            previousWeek = state[week];
        }
        return false;
    }
}