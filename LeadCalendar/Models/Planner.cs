namespace LeadCalendar.Models;

public struct Combination
{
    public int[] Weeks { get; init; }
}

public class Planner
{
    private readonly int _weeksCount;
    private readonly int _weeksPerAgent;
    private Agent[] _agents;
    private readonly Combination[][] _weekCombinations;
    
    private List<FormedPlan> _validPlans = [];
    private List<FormedPlan> _conflictingPlans = [];
    private List<PlanInProgress> _plansInProgress = [];
    private List<PlanInProgress> _conflictingPlansInProgress = [];
    
    // Stats
    private int _totalPlansCount = 0;
    private int _invalidPlansCount = 0;
    
    public Planner(int weeksCount, int weeksPerAgent, int numberOfAgents)
    {
        _weeksCount = weeksCount;
        _weeksPerAgent = weeksPerAgent;
        _agents = new Agent[numberOfAgents];
        _weekCombinations = new Combination[numberOfAgents][];
    }
    
    
}