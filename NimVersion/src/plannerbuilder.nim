import std/[sequtils]
import ./planner
import ./statecombination
import ./combinationshelper

type
    PlannerBuilder* = ref object
        weeksCount: uint8
        weeksPerAgent: uint8
        minAgentsPerWeek: uint8
        agentNames: seq[string]
        excludedWeeksPerAgent: seq[seq[uint8]]
        previousWeekAgentIds: seq[uint8]


proc newPlannerBuilder*(weeksCount: uint8, weeksPerAgent: uint8, minAgentsPerWeek: uint8): PlannerBuilder =
    if weeksCount < 2:
        raise newException(Exception, "Number of weeks must be at least 2")
    if minAgentsPerWeek == 0:
        raise newException(Exception, "Minimum number of agents per week must be greater than 0")
    if weeksPerAgent == 0:
        raise newException(Exception, "Number of weeks per agent must be greater than 0")
    if weeksPerAgent > weeksCount: 
        raise newException(Exception, "Required number of weeks per agent must be less than the number of weeks")

    return PlannerBuilder(
        weeksCount: weeksCount,
        weeksPerAgent: weeksPerAgent,
        minAgentsPerWeek: minAgentsPerWeek,
        agentNames: @[],
        excludedWeeksPerAgent: @[],
        previousWeekAgentIds: @[],
    )


proc addAgent*(builder: PlannerBuilder, name: string, excludedWeeks: varargs[uint8]): PlannerBuilder =
    if excludedWeeks.len > builder.weeksCount.int:
        raise newException(Exception, "Excluded weeks count must be less than the number of weeks")
    if builder.weeksCount.int - excludedWeeks.len < builder.minAgentsPerWeek.int:
        echo "WARN: Not enough open weeks for ", name
    
    builder.agentNames.add(name)
    builder.excludedWeeksPerAgent.add(@excludedWeeks)

    # Combinations can't be generated at this point,
    # because the previous week isn't known yet.

    return builder


proc setPreviousWeek*(builder: PlannerBuilder, agentNames: varargs[string]): PlannerBuilder =
    for agentName in agentNames:
        let agentId = builder.agentNames.find(agentName)
        if agentId == -1:
            raise newException(Exception, "Agent with name " & agentName & " not found")
        builder.previousWeekAgentIds.add(agentId.uint8)

    return builder


proc getEmptyWeeks(builder: PlannerBuilder, agentId: uint8): seq[uint8] =
    let excludedWeeks = builder.excludedWeeksPerAgent[agentId]
    var emptyWeeks: seq[uint8] = @[]
    for weekIndex in 1..builder.weeksCount.uint32:
        if excludedWeeks.contains(weekIndex.uint8): continue
        emptyWeeks.add(weekIndex.uint8)
    return emptyWeeks


proc calculateInitialState(builder: PlannerBuilder, agentId: uint8): seq[bool] =
    let isInPreviousWeek = builder.previousWeekAgentIds.contains(agentId)
    var state: seq[bool] = newSeq[bool](builder.weeksCount + 1)
    state[0] = isInPreviousWeek
    return state


proc generateStateCombinations(builder: PlannerBuilder, agentId: uint8): tuple[stateCombinations: seq[StateCombination], firstConflictIndex: uint8] =
    let agentName = builder.agentNames[agentId]
    let emptyWeeks = builder.getEmptyWeeks(agentId)
    let initialState = builder.calculateInitialState(agentId)

    # Handle cases when there aren't enough open weeks for the agent
    if emptyWeeks.len == 0:
        raise newException(Exception, "Agent " & agentName & " has no open weeks")
    if emptyWeeks.len <= builder.weeksPerAgent.int:
        echo "WARN: Agent ", agentName, " has only ", emptyWeeks.len, " open weeks - returning single incomplete combination"
        # Simply return a single combination with all the open weeks
        var combination = initialState.applyCombination(emptyWeeks)
        # Mark it as not-conflicting it's not even a valid combination
        combination.hasConflict = false
        return (stateCombinations: @[combination], firstConflictIndex: 1)

    # Generate combinations
    let combinations = calcCombinations[uint8](emptyWeeks, builder.weeksPerAgent)
    var stateCombinations: seq[StateCombination] = @[]
    var firstConflictIndex: uint8 = 0
    for combination in combinations:
        let stateCombination = initialState.applyCombination(combination)
        if stateCombination.hasConflict:
            stateCombinations.add(stateCombination)
        else:
            stateCombinations.insert(stateCombination, 0)
            inc firstConflictIndex

    return (stateCombinations: stateCombinations, firstConflictIndex: firstConflictIndex)


proc processCombinationsSet(builder: PlannerBuilder, agentId : uint8, stateCombinations : var seq[StateCombination], firstConflictIndex : uint8) =
    ## Performas checks on the generated combinations. If there are unavoidable conflicts,
    ## all combinations are marked as not-conflicting. This is done to keep avoiding additional conflicts
    ## which keeps the planner efficient.
    let agentName = builder.agentNames[agentId]
    echo "Checking agent ", agentName
    var hasValidCombination = false
    for combination in stateCombinations:
        echo " - Has combination: ", combination
        if not combination.hasConflict:
            hasValidCombination = true

    if not hasValidCombination:
        echo "WARN: Agent ", agentName, " has unavoidable conflicts - marking all combinations as not-conflicting"
        # Mark all combinations as not-conflicting to keep things optimized
        # and avoid unnecessary error messages - it's unavoidable after all
        for combinationId in 0..<stateCombinations.len:
            var combination = stateCombinations[combinationId]
            combination.hasConflict = false
            stateCombinations[combinationId] = combination


proc build*(builder: PlannerBuilder): Planner =
    if builder.agentNames.len < 2:
        raise newException(Exception, "At two agents must be added")
    if builder.agentNames.len < builder.minAgentsPerWeek.int:
        raise newException(Exception, "There aren't enough agents to satisfy the minimum number of agents per week requirement")

    var combinationsPerAgent: seq[seq[StateCombination]] = @[]
    var firstConflictIndexes: seq[uint8] = @[]

    for agentId in 0..<builder.agentNames.len:
        var (stateCombinations, firstConflictId) = builder.generateStateCombinations(agentId.uint8)
        builder.processCombinationsSet(agentId.uint8, stateCombinations, firstConflictId)
        combinationsPerAgent.add(stateCombinations)
        firstConflictIndexes.add(firstConflictId)

    let agentStateCombinations: seq[AgentStateCombinations] = combinationsPerAgent.mapIt(AgentStateCombinations(combinations: it))

    return newPlanner(
        builder.weeksCount,
        builder.weeksPerAgent,
        builder.minAgentsPerWeek,
        builder.agentNames,
        agentStateCombinations,
        firstConflictIndexes,
    )