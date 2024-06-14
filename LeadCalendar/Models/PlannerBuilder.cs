using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public class PlannerBuilder
{
    private readonly int _weeksCount;
    private readonly int _weeksPerAgent;
    private readonly int _minAgentsPerWeek;
    private List<string> _agentNames = [];
    private List<int[]> _excludedWeeksPerAgent = [];
    private List<int> _previousWeekAgentIds = [];
    
    public PlannerBuilder(int weeksCount, int weeksPerAgent, int minAgentsPerWeek)
    {
        _weeksCount = weeksCount;
        _weeksPerAgent = weeksPerAgent;
        _minAgentsPerWeek = minAgentsPerWeek;
    }

    public PlannerBuilder AddAgent(string name, params int[] excludedWeeks)
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
            
            _previousWeekAgentIds.Add(agentId);
        }
           
        return this;
    }

    public Planner Build()
    {

        var combinationsPerAgent = new List<StateCombination[]>();
        var firstConflictIndexes = new List<int>();

        for (var agentId = 0; agentId < _agentNames.Count; agentId++)
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

    private int[] GetEmptyWeeks(int agentId)
    {
        var excludedWeeks = _excludedWeeksPerAgent[agentId];
        var emptyWeeks = new List<int>();
        for (var weekIndex = 1; weekIndex <= _weeksCount; weekIndex++)
        {
            if (excludedWeeks.Contains(weekIndex)) continue;
            emptyWeeks.Add(weekIndex);
        }
        return emptyWeeks.ToArray();
    }

    private (StateCombination[], int) GenerateStateCombinations(int agentId)
    {
        var emptyWeeks = GetEmptyWeeks(agentId);
        var combinations = CombinationsHelper.GenerateCombinations(emptyWeeks, _weeksPerAgent).ToArray();
        var stateCombinations = new List<StateCombination>();
        var initialState = CalculateInitialState(agentId);
        var firstConflictIndex = 0;
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
    private bool[] CalculateInitialState(int agentId)
    {
        var isInPreviousWeek = _previousWeekAgentIds.Contains(agentId);
        var state = new bool[_weeksCount + 1];
        state[0] = isInPreviousWeek;
        return state;
    }

    private StateCombination ApplyCombination(bool[] initialState, int[] combination)
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
        for (var week = 1; week <= _weeksCount; week++)
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