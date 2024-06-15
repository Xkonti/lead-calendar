import std/[algorithm, strutils]
import ./statecombination

type
    FrozenState* = ref object
        parentFrozenState*: FrozenState
        lastStateCombinationId*: uint8
        statesCount*: uint8
        hasConflict*: bool
        selectionsPerWeek*: seq[uint8]


proc newFrozenState*(weeksCount: uint8): FrozenState =
    return FrozenState(
        parentFrozenState: nil,
        lastStateCombinationId: 0,
        statesCount: 0,
        hasConflict: false,
        selectionsPerWeek: newSeq[uint8](weeksCount))

proc increment*(selectionsPerWeek: seq[uint8], weekSelections: seq[bool]): seq[uint8] =
    var newSelections = selectionsPerWeek
    for weekId in 0..<selectionsPerWeek.len:
        if weekSelections[weekId+1]: inc newSelections[weekId]
    
    return newSelections

proc appendState*(parentState: FrozenState, stateCombinationId: uint8, state: StateCombination): FrozenState =
    return FrozenState(
        parentFrozenState: parentState,
        lastStateCombinationId: stateCombinationId,
        statesCount: parentState.statesCount + 1,
        hasConflict: parentState.hasConflict or state.hasConflict,
        selectionsPerWeek: parentState.selectionsPerWeek.increment(state.weekSelections))

proc getStatCombinationIds*(state: FrozenState): seq[uint8] =
    result = newSeq[uint8](state.statesCount)
    var currentState = state
    for i in 0..<state.statesCount.int:
        result[i] = currentState.lastStateCombinationId
        currentState = currentState.parentFrozenState
    
    result.reverse()


proc getStatePerAgent*(state: FrozenState, combinationsPerAgent: seq[AgentStateCombinations]): seq[seq[bool]] =
    let agentCount = combinationsPerAgent.len
    let stateIds = state.getStatCombinationIds()
    if agentCount != stateIds.len:
        raise newException(Exception, "State and combinationsPerAgent must have the same length")
    result = newSeq[seq[bool]](agentCount)
    for agentId in 0..<agentCount:
        result[agentId] = combinationsPerAgent[agentId].combinations[stateIds[agentId]].weekSelections


proc presentAsList*(state: FrozenState, agentNames: seq[string], combinationsPerAgent: seq[AgentStateCombinations]): string =
    let states = state.getStatePerAgent(combinationsPerAgent)
    for weekId in 1..state.selectionsPerWeek.len:
        result &= "Week " & $weekId & ": "
        var assignedNames = newSeq[string]()
        for agentId in 0..<agentNames.len:
            if states[agentId][weekId]: assignedNames.add(agentNames[agentId])
        result &= assignedNames.join(", ")
        result &= "\n"