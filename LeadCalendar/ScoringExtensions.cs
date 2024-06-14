using LeadCalendar.Helpers;
using LeadCalendar.Models;

namespace LeadCalendar;

public static class ScoringExtensions
{
    public static IEnumerable<FrozenState> SelectBest(this IReadOnlyCollection<FrozenState> frozenStates,
        int targetCount, Func<FrozenState, int> scorer)
    {
        var resultList = new LinkedList<FrozenState>();

        var minScore = int.MaxValue;
        var maxScore = 0;
        
        // Simply go through the list, score each plan, and maintain the minimum (targetCount)number of plans
        foreach (var frozenState in frozenStates)
        {
            var score = scorer(frozenState);

            if (score <= minScore)
            {
                minScore = score;
                resultList.AddFirst(frozenState);
                if (resultList.Count > targetCount)
                    resultList.RemoveLast();
            }
            else if (score > maxScore && resultList.Count < targetCount)
            {
                maxScore = score;
                resultList.AddLast(frozenState);
            }
        }
        
        return resultList;
    }
}