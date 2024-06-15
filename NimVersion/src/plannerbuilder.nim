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
    return PlannerBuilder(
        weeksCount: weeksCount,
        weeksPerAgent: weeksPerAgent,
        minAgentsPerWeek: minAgentsPerWeek,
        agentNames: @[],
        excludedWeeksPerAgent: @[],
        previousWeekAgentIds: @[],
    )

proc addAgent*(builder: PlannerBuilder, name: string, excludedWeeks: varargs[uint8]): PlannerBuilder =
    builder.agentNames.add(name)
    builder.excludedWeeksPerAgent.add(@excludedWeeks)
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
    let emptyWeeks = builder.getEmptyWeeks(agentId)
    let combinations = calcCombinations[uint8](emptyWeeks, builder.weeksPerAgent)
    var stateCombinations: seq[StateCombination] = @[]
    let initialState = builder.calculateInitialState(agentId)
    var firstConflictIndex: uint8 = 0
    for combination in combinations:
        let stateCombination = initialState.applyCombination(combination)
        if stateCombination.hasConflict:
            stateCombinations.add(stateCombination)
        else:
            stateCombinations.insert(stateCombination, 0)
            inc firstConflictIndex

    return (stateCombinations: stateCombinations, firstConflictIndex: firstConflictIndex)


proc build*(builder: PlannerBuilder): Planner =
    var combinationsPerAgent: seq[seq[StateCombination]] = @[]
    var firstConflictIndexes: seq[uint8] = @[]

    for agentId in 0..<builder.agentNames.len:
        let (stateCombinations, firstConflictId) = builder.generateStateCombinations(agentId.uint8)
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