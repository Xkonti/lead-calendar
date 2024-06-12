using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public class PlannerBuilder
{
    private readonly int _weeksCount;
    private readonly int _weeksPerAgent;
    private List<Agent> _agents = [];
    private List<int> _previousWeekAgentIds = [];
    
    public PlannerBuilder(int weeksCount, int weeksPerAgent)
    {
        _weeksCount = weeksCount;
        _weeksPerAgent = weeksPerAgent;
    }

    public PlannerBuilder AddAgent(Agent agent)
    {
        _agents.Add(agent);
        return this;
    }
    
    public PlannerBuilder SetPreviousWeek(params string[] agentNames)
    {
        foreach (var agentName in agentNames)
        {
            var agentId = _agents.FindIndex(a => a.Name == agentName);
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
        // Generate initial plan in progress
        // It needs to include the previous week as week 0
        var states = new List<AgentState>();
        
        for (var i = 0; i < _agents.Count; i++)
        {
            var selectedWeeks = new List<int>();
            if (_previousWeekAgentIds.Contains(i))
                selectedWeeks.Add(0);
            states.Add(new AgentState(i, _weeksCount, selectedWeeks.ToArray()));
        }
        
        var initialPlan = new PlanInProgress { AgentStates = states.ToArray(), LockedStates = 0 };
        
        // Generate combinations
        var combinationsPerState = new List<Combination<int>[]>();
        foreach (var state in states)
        {
            var items = new List<int>();
            var excludedWeeks = _agents[state.AgentId].ExcludedWeeks;
            for (var weekIndex = 1; weekIndex <= _weeksCount; weekIndex++)
            {
                if (excludedWeeks.Contains(weekIndex)) continue;
                items.Add(weekIndex);
            }
            var combinations = CombinationsHelper.GenerateCombinations(items.ToArray(), _weeksPerAgent);
            combinationsPerState.Add(combinations.ToArray());
        }
        
        return new Planner(_weeksCount, _weeksPerAgent, _agents.ToArray(), combinationsPerState.ToArray(), initialPlan);
    }
}