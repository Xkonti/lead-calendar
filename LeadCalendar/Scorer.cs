using LeadCalendar.Helpers;
using LeadCalendar.Models;

namespace LeadCalendar;

public sealed class Scorer
{
    private const int ConflictCountMultiplier = 10000;
    private const int DeviationMultiplier = 200;
    private const int DeviationAvgMultiplier = 100;
    
    public StateCombinationScore[][] CombinationScoresPerAgent { get; init; }

    public Scorer(AgentStateCombinations[] combinationsPerAgent)
    {
        CombinationScoresPerAgent = CalculateCombinationScores(combinationsPerAgent);
    }

    public StateCombinationScore GetCombinationScore(int agentIndex, int combinationId)
    {
        return CombinationScoresPerAgent[agentIndex][combinationId];
    }

    public int CalculatePlanScore(FrozenState plan)
    {
        var conflictScore = CalculatePlanConflictScore(plan);
        var deviationScore = CalculatePlanDeviationScore(plan);
        return conflictScore + deviationScore;
    }

    public int CalculatePlanConflictScore(FrozenState plan)
    {
        var stateIds = plan.GetStateCombinationIds();
        var score = 0;
        for (var agentId = 0; agentId < stateIds.Length; agentId++)
        {
            score += GetCombinationScore(agentId, stateIds[agentId]).ConflictScore;
        }

        return score;
    }

    public int CalculatePlanDeviationScore(FrozenState plan)
    {
        var minAgentsPerWeek = int.MaxValue;
        var maxAgentsPerWeek = 0;
        var sum = 0.0f;
        for (var weekId = 0; weekId < plan.SelectionsPerWeek.Length; weekId++)
        {
            var selected = plan.SelectionsPerWeek[weekId];
            sum += selected;
            if (selected < minAgentsPerWeek) minAgentsPerWeek = selected;
            if (selected > maxAgentsPerWeek) maxAgentsPerWeek = selected;
        }
        
        var deviation = maxAgentsPerWeek - minAgentsPerWeek;
        var deviationScore = deviation * DeviationMultiplier;
        
        var avg = sum / plan.SelectionsPerWeek.Length;
        var deviationDown = maxAgentsPerWeek - avg;
        var deviationUp = avg - minAgentsPerWeek;
        var maxDeviation = Math.Max(deviationDown, deviationUp);
        var deviationAvgScore = maxDeviation * DeviationAvgMultiplier;
        return deviationScore + (int)deviationAvgScore;
    }

    private static StateCombinationScore[][] CalculateCombinationScores(AgentStateCombinations[] combinationsPerAgent)
    {
        var result = new StateCombinationScore[combinationsPerAgent.Length][];
        for (var agentId = 0; agentId < combinationsPerAgent.Length; agentId++)
        {
            var combinationsCount = combinationsPerAgent[agentId].Combinations.Length;
            result[agentId] = new StateCombinationScore[combinationsCount];
            for (var combinationId = 0; combinationId < combinationsCount; combinationId++)
            {
                result[agentId][combinationId] = ScoreCombination(combinationsPerAgent[agentId].Combinations[combinationId]);
            }
        }
        
        return result;
    }

    private static StateCombinationScore ScoreCombination(StateCombination combination)
    {
        if (!combination.HasConflict) return new StateCombinationScore { ConflictScore = 0 };
        var conflictCount = 0;
        var isPreviousSelected = combination.WeekSelections[0];
        for (var i = 1; i < combination.WeekSelections.Length; i++)
        {
            var currentSelected = combination.WeekSelections[i];
            if (isPreviousSelected && currentSelected) conflictCount++;
            isPreviousSelected = currentSelected;
        }

        return new StateCombinationScore
        {
            ConflictScore = ConflictCountMultiplier * conflictCount
        };
    }
}