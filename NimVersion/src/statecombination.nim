import std/[strutils, sequtils]

type
    StateCombination* = object
        weekSelections*: seq[bool]
        hasConflict*: bool

    AgentStateCombinations* = object
        combinations*: seq[StateCombination]

proc detectConflict*(state: seq[bool]): bool =
    # Check each week if it has conflict with previous week
    var previousWeek = state[0]
    for weekId in 1..<state.len:
        if previousWeek and state[weekId]:
            return true
        previousWeek = state[weekId]
    return false

proc applyCombination*(initialState: seq[bool], combination: seq[uint8]): StateCombination =
    var state = initialState
    for weekId in combination:
        state[weekId] = true
    let hasConflict = state.detectConflict()
    return StateCombination(weekSelections: state, hasConflict: hasConflict)

proc `$`*(combination: StateCombination): string =
    return combination.weekSelections.mapIt(if it: "X" else: "_").join(" ") & (if combination.hasConflict: " CONF" else: "")