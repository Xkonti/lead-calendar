namespace LeadCalendar.Models;

public class Planner
{
    public int WeeksCount { get; init; }
    public int WeeksPerAgent { get; init; }
    
    public string[] AgentNames { get; init; }
    
    public AgentStateCombinations[] CombinationsPerAgent { get; init; }
    
    /// <summary>
    /// Indexes of the first conflicting combination for each agent.
    /// This is used to skip conflicting combinations when at least one valid plan has been found.
    /// </summary>
    public int[] FirstConflictIndexes { get; init; }

    private List<PendingPlan> _pendingValidPlans = [];
    private List<PendingPlan> _pendingConflictingPlans = [];
    
    private List<PendingPlan> _validPlans = [];
    private List<PendingPlan> _conflictingPlans = [];
    
    // Stats
    private int _totalPlansCount = 0;
    private int _invalidPlansCount = 0;
    
    public Planner(
        int weeksCount, int weeksPerAgent,
        string[] agentNames,
        AgentStateCombinations[] combinationsPerAgent,
        int[] firstConflictIndexes
        )
    {
        // Verify agent state combinations
        
        if (agentNames.Length != combinationsPerAgent.Length)
        {
            throw new ArgumentException("AgentNames and StateCombinations must have the same length");
        }
        
        foreach (var agentStateCombinations in combinationsPerAgent)
        {
            if (agentStateCombinations.Combinations.Length == 0)
                throw new ArgumentException("Each agent has to have at least one state combination available");

            if (agentStateCombinations.Combinations.Any(combination => combination.WeekSelections.Length != weeksCount + 1))
                throw new ArgumentException("Each combination must have a valid number of weeks");
        }
        
        if (agentNames.Length != firstConflictIndexes.Length)
            throw new ArgumentException("AgentNames and FirstConflictIndexes must have the same length");
        
        // Save data
        
        WeeksCount = weeksCount;
        WeeksPerAgent = weeksPerAgent;
        AgentNames = agentNames;
        CombinationsPerAgent = combinationsPerAgent;
        FirstConflictIndexes = firstConflictIndexes;
    }
    

    public void PrintStats()
    {
        Console.WriteLine($"TOT: {_totalPlansCount} INV: {_invalidPlansCount} CONF: {_conflictingPlans.Count} VAL: {_validPlans.Count}");
        Console.WriteLine($"In progress: {_pendingValidPlans.Count} Conflicting: {_pendingConflictingPlans.Count}");
    }

    public bool CheckHasUnavoidableConflicts()
    {
        var conflictingAgentNames = new List<string>();
        
        for (var agentId = 0; agentId < AgentNames.Length; agentId++)
        {
            Console.WriteLine($"Checking agent {AgentNames[agentId]}");
            var agentStateCombinations = CombinationsPerAgent[agentId];
            foreach (var combination in agentStateCombinations.Combinations)
            {
                Console.WriteLine($" - Has combination: {combination}");
            }
            var hasConflictsOnly = agentStateCombinations.Combinations.All(combination => combination.HasConflict);
            if (hasConflictsOnly)
            {
                conflictingAgentNames.Add(AgentNames[agentId]);
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