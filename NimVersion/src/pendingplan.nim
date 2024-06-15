import ./frozenstate

type
    PendingPlan* = object
        frozenState*: FrozenState
        selectedCombination*: uint8