namespace LeadCalendar.Models;

public struct AgentState
{
    public int AgentId { get; init; }
    public bool[] SelectedWeeks { get; init; }
    
    public int SelectedWeeksCount { get; init; }

    public AgentState(int agentId, int totalWeeks, params int[] selectedWeeks)
    {
        AgentId = agentId;
        SelectedWeeks = new bool[totalWeeks+1];
        foreach (var week in selectedWeeks)
        {
            SelectedWeeks[week] = true;
        }
        SelectedWeeksCount = selectedWeeks.Length;
    }

    public bool IsSelected(int weekIndex)
    {
        return SelectedWeeks[weekIndex];
    }

    public bool IsEmpty(int weekIndex, Planner planner)
    {
        if (SelectedWeeks[weekIndex]) return false;
        if (planner.Agents[AgentId].ExcludedWeeks.Contains(weekIndex)) return false;
        return true;
    }

    public AgentState Apply(Combination<int> combination)
    {
        // Copy weeks to a new array
        var newSelectedWeeks = (bool[])SelectedWeeks.Clone();
        var selectedCount = SelectedWeeksCount;
        foreach (var week in combination.Weeks)
        {
            if (newSelectedWeeks[week]) continue;
            newSelectedWeeks[week] = true;
            selectedCount++;
        }
        
        return new AgentState { AgentId = AgentId, SelectedWeeks = newSelectedWeeks, SelectedWeeksCount = selectedCount };
    }

    public AgentStateStatus GetStatus(Planner planner)
    {
        // Full validity checks
        if (SelectedWeeksCount == 2)
        {
            var isPreviousWeekSelected = SelectedWeeks[0];
            var hasConflict = false;

            for (var weekId = 1; weekId < planner.WeeksCount; weekId++)
            {
                // Check for assigned excluded week
                var isSelected = SelectedWeeks[weekId];
                if (isSelected)
                {
                    if (planner.Agents[AgentId].ExcludedWeeks.Contains(weekId))
                    {
                        return AgentStateStatus.Invalid;
                    }
                    
                    if (isPreviousWeekSelected)
                    {
                        hasConflict = true;
                    }

                    isPreviousWeekSelected = true;
                    continue;
                }
                
                isPreviousWeekSelected = false;
            }

            return hasConflict
                ? AgentStateStatus.ValidWithConflicts
                : AgentStateStatus.Valid;
        }
        
        // Not fully valid
        if (SelectedWeeksCount > 2) return AgentStateStatus.Invalid;
        return AgentStateStatus.Incomplete;
    }
}