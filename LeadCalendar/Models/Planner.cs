namespace LeadCalendar.Models;

public class Planner
{
    public int WeeksCount { get; init; }
    public int WeeksPerAgent { get; init; }
    public Agent[] Agents { get; init; }
    public Combination<int>[][] WeekCombinations { get; init; }
    
    private List<FormedPlan> _validPlans = [];
    private List<FormedPlan> _conflictingPlans = [];
    private List<PlanInProgress> _plansInProgress = [];
    private List<PlanInProgress> _conflictingPlansInProgress = [];
    
    // Stats
    private int _totalPlansCount = 0;
    private int _invalidPlansCount = 0;
    
    public Planner(
        int weeksCount, int weeksPerAgent,
        Agent[] agents, Combination<int>[][] weekCombinations,
        PlanInProgress initialPlan)
    {
        WeeksCount = weeksCount;
        WeeksPerAgent = weeksPerAgent;
        Agents = agents;
        WeekCombinations = weekCombinations;
        _plansInProgress.Add(initialPlan);
    }
    
    public PlanInProgress GetCurrentPlan()
    {
        return _plansInProgress[0];
    }
    
    public void AddPlan(FormedPlan plan)
    {
        _validPlans.Add(plan);
        _totalPlansCount++;
    }

    public void PrintStats()
    {
        Console.WriteLine($"TOT: {_totalPlansCount} INV: {_invalidPlansCount} CONF: {_conflictingPlans.Count} VAL: {_validPlans.Count}");
        Console.WriteLine($"In progress: {_plansInProgress.Count} Conflicting: {_conflictingPlansInProgress.Count}");
    }

    public bool CheckHasUnavoidableConflicts()
    {
        var conflictingAgentNames = new List<string>();
        var plan = GetCurrentPlan();
        for (var agentId = 0; agentId < WeekCombinations.Length; agentId++)
        {
            var weekCombinations = WeekCombinations[agentId];
            var state = plan.AgentStates[agentId];
            var isConflict = weekCombinations.All(combination => state.Apply(combination).GetStatus(this) == AgentStateStatus.ValidWithConflicts);
            if (isConflict)
            {
                conflictingAgentNames.Add(Agents[agentId].Name);
            }
        }

        if (conflictingAgentNames.Count == 0)
        {
            Console.WriteLine("No unavoidable conflicts detected");
            return false;
        }
        
        Console.WriteLine($"Unavoidable conflicts detected for agents: {string.Join(", ", conflictingAgentNames)}");
        return true;
    }
}