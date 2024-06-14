using LeadCalendar.Helpers;

namespace LeadCalendar.Models;

public sealed class Planner
{
    public byte WeeksCount { get; init; }
    public byte WeeksPerAgent { get; init; }
    public byte MinAgentsPerWeek { get; init; }
    
    public string[] AgentNames { get; init; }
    
    public AgentStateCombinations[] CombinationsPerAgent { get; init; }
    
    public Scorer Scorer { get; init; }
    
    /// <summary>
    /// Indexes of the first conflicting combination for each agent.
    /// This is used to skip conflicting combinations when at least one valid plan has been found.
    /// </summary>
    public byte[] FirstConflictIndexes { get; init; }

    private List<PendingPlan> _pendingValidPlans = [];
    private List<PendingPlan> _pendingConflictingPlans = [];
    
    private List<FrozenState> _validPlans = [];
    private List<FrozenState> _conflictingPlans = [];
    
    // Stats
    private int _totalPlansCount = 0;
    private int _invalidPlansCount = 0;
    
    public List<FrozenState> ResultingPlans
    {
        get
        {
            var result = new List<FrozenState>();
            result.AddRange(_validPlans);
            if (_validPlans.Count != 0) return result;
            result.AddRange(_conflictingPlans);
            return result;
        }
    }

    public Planner(
        byte weeksCount, byte weeksPerAgent, byte minAgentsPerWeek,
        string[] agentNames,
        AgentStateCombinations[] combinationsPerAgent,
        byte[] firstConflictIndexes
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
        MinAgentsPerWeek = minAgentsPerWeek;
        AgentNames = agentNames;
        CombinationsPerAgent = combinationsPerAgent;
        FirstConflictIndexes = firstConflictIndexes;
        
        Scorer = new Scorer(CombinationsPerAgent);
        
        // Generate first possible plans
        for (byte combinationId = 0; combinationId < CombinationsPerAgent[0].Combinations.Length; combinationId++)
        {
            var newPlan = new PendingPlan
            {
                FrozenState = new FrozenState(WeeksCount),
                SelectedCombination = combinationId,
            };
            
            var combination = CombinationsPerAgent[0].Combinations[combinationId];
            if (!combination.HasConflict) _pendingValidPlans.Add(newPlan);
            else _pendingConflictingPlans.Add(newPlan);
            _totalPlansCount++;
        }
    }
    

    public void PrintStats()
    {
        Console.WriteLine($"TOT: {_totalPlansCount} INV: {_invalidPlansCount} CONF: {_conflictingPlans.Count} VAL: {_validPlans.Count}");
        Console.WriteLine($"In progress: {_pendingValidPlans.Count} Conflicting: {_pendingConflictingPlans.Count}");
    }

    public bool CheckHasUnavoidableConflicts()
    {
        var conflictingAgentNames = new List<string>();
        
        for (byte agentId = 0; agentId < AgentNames.Length; agentId++)
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

    /// <summary>
    /// Finds valid plans and updates the internal state.
    /// </summary>
    /// <returns>Returns false if there are no more iterations to take.</returns>
    public bool FindPlansLoopIteration()
    {
        // Get next plan
        var planResult = GetNextPlan();
        if (planResult == null)
            return false;
        
        // Advance state of the plan
        var plan = planResult.Value;
        var agentId = plan.FrozenState.StatesCount;
        var newState = plan.FrozenState
            .AppendState(plan.SelectedCombination, CombinationsPerAgent[agentId].Combinations[plan.SelectedCombination]);
        
        // If plan is finished, add to appropriate list
        if (agentId == AgentNames.Length - 1)
        {
            // Detect invalid plans vy verifying state week-wise
            for (byte week = 0; week < newState.SelectionsPerWeek.Length; week++)
            {
                if (newState.SelectionsPerWeek[week] >= MinAgentsPerWeek) continue;
                _invalidPlansCount++;
                return true;
            }

            if (!newState.HasConflict)
            {
                _validPlans.Add(newState);
                return true;
            }

            // Ignore conflicting plans if there is a valid plan
            if (_validPlans.Count == 0)
            {
                _conflictingPlans.Add(newState);
            }
            
            return true;
        }
        
        // The plan is not finished, generate next possible plans
        
        // Valid plans
        var firstConflictIndex = FirstConflictIndexes[agentId + 1];
        for (byte combinationId = 0; combinationId < firstConflictIndex; combinationId++)
        {
            var nextPlan = new PendingPlan { FrozenState = newState, SelectedCombination = combinationId };
            _pendingValidPlans.Add(nextPlan);
            _totalPlansCount++;
        }

        // Optionally conflicting plans
        if (_validPlans.Count != 0) return true;
        var totalCombinations = CombinationsPerAgent[agentId + 1].Combinations.Length;
        
        // Full plans should be added to the valid queue to complete them asap
        if (agentId + 1 == AgentNames.Length)
        {
            for (byte combinationId = firstConflictIndex; combinationId < totalCombinations; combinationId++)
            {
                var nextPlan = new PendingPlan { FrozenState = newState, SelectedCombination = combinationId };
                _pendingValidPlans.Add(nextPlan);
                _totalPlansCount++;
            }

            return true;
        }
        
        for (byte combinationId = firstConflictIndex; combinationId < totalCombinations; combinationId++)
        {
            var nextPlan = new PendingPlan { FrozenState = newState, SelectedCombination = combinationId };
            _pendingConflictingPlans.Add(nextPlan);
            _totalPlansCount++;
        }

        return true;
    }

    public void LimitPlans(int targetCount = 1000)
    {
        if (_validPlans.Count > targetCount)
        {
            _validPlans.Shuffle(); // Randomize the order so that we get some variety in results
            _validPlans = _validPlans.SelectBest(targetCount, Scorer.CalculatePlanDeviationScore).ToList();
            if (_conflictingPlans.Count > 0) _conflictingPlans = [];
            if (_pendingConflictingPlans.Count > 0) _pendingConflictingPlans = [];
        }
        
        if (_conflictingPlans.Count > targetCount)
        {
            _conflictingPlans.Shuffle(); // Randomize the order so that we get some variety in results
            _conflictingPlans = _conflictingPlans.SelectBest(targetCount, Scorer.CalculatePlanScore).ToList();
        }
    }

    private PendingPlan? GetNextPlan()
    {
        if (_pendingValidPlans.Count > 0)
        {
            var plan = _pendingValidPlans[_pendingValidPlans.Count - 1];
            _pendingValidPlans.RemoveAt(_pendingValidPlans.Count - 1);
            return plan;
        }
        
        if (_pendingConflictingPlans.Count > 0)
        {
            var plan = _pendingConflictingPlans[_pendingConflictingPlans.Count - 1];
            _pendingConflictingPlans.RemoveAt(_pendingConflictingPlans.Count - 1);
            return plan;
        }
        
        return null;
    }
}