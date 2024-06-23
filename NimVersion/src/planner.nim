import std/[deques, options, random, sequtils, sugar]
import ./statecombination
import ./frozenstate
import ./pendingplan
import ./scorer

type
    Planner* = ref object
        weeksCount*: uint8
        weeksPerAgent*: uint8
        minAgentsPerWeek*: uint8
        agentNames*: seq[string]
        combinationsPerAgent*: seq[AgentStateCombinations]
        firstConflictIndexes*: seq[uint8]

        pendingValidPlans: seq[PendingPlan] = @[]
        pendingConflictingPlans: seq[PendingPlan] = @[]

        validPlans: seq[FrozenState] = @[]
        conflictingPlans: seq[FrozenState] = @[]

        totalPlansCount: int = 0
        invalidPlansCount: int = 0
        
        scorer*: Scorer


proc newPlanner*(
    weeksCount: uint8, weeksPerAgent: uint8, minAgentsPerWeek: uint8,
    agentNames: seq[string],
    combinationsPerAgent: seq[AgentStateCombinations],
    firstConflictIndexes: seq[uint8]): Planner =
    
    # Verify agent state combinations
    if agentNames.len != combinationsPerAgent.len:
        raise newException(Exception, "AgentNames and StateCombinations must have the same length")

    if agentNames.len != firstConflictIndexes.len:
        raise newException(Exception, "AgentNames and FirstConflictIndexes must have the same length")

    # Save data
    var planner = Planner(
        weeksCount: weeksCount,
        weeksPerAgent: weeksPerAgent,
        minAgentsPerWeek: minAgentsPerWeek,
        agentNames: agentNames,
        combinationsPerAgent: combinationsPerAgent,
        firstConflictIndexes: firstConflictIndexes,
    )

    # Generate first possible plans
    for combinationId in 0..<combinationsPerAgent[0].combinations.len:
        let newPlan = PendingPlan(
            frozenState: newFrozenState(weeksCount.uint8),
            selectedCombination: combinationId.uint8,
        )

        var combination = combinationsPerAgent[0].combinations[combinationId]
        if not combination.hasConflict:
            planner.pendingValidPlans.add(newPlan)
        else:
            planner.pendingConflictingPlans.add(newPlan)
        inc planner.totalPlansCount

    randomize(678)
    planner.scorer = newScorer(combinationsPerAgent)
    return planner


proc printStats*(planner: Planner) =
    echo "TOT: ", planner.totalPlansCount, " INV: ", planner.invalidPlansCount, " CONF: ", planner.conflictingPlans.len, " VAL: ", planner.validPlans.len
    echo "In progress: ", planner.pendingValidPlans.len, " Conflicting: ", planner.pendingConflictingPlans.len


proc getNextPlan(planner: Planner): Option[PendingPlan] =
    if planner.pendingValidPlans.len > 0:
        return some(planner.pendingValidPlans.pop())
    if planner.pendingConflictingPlans.len > 0:
        return some(planner.pendingConflictingPlans.pop())
    return none(PendingPlan)


proc findPlansLoopIteration*(planner: Planner): bool =
    ## Finds valid plans and updates the internal state.
    ## Returns false if there are no more iterations to take.
    
    # Get next plan
    var planResult = planner.getNextPlan()
    if planResult.isNone:
        return false
    let plan = planResult.get()

    # Advance state of the plan
    let agentId = plan.frozenState.statesCount
    let newState = plan.frozenState.appendState(plan.selectedCombination, planner.combinationsPerAgent[agentId].combinations[plan.selectedCombination])

    # If plan is finished, add to appropriate list
    if agentId.int == planner.agentNames.len - 1:
        # Detect invalid plans by verifying state week-wise
        for weekId in 0..<newState.selectionsPerWeek.len:
            if newState.selectionsPerWeek[weekId] >= planner.minAgentsPerWeek:
                continue
            inc planner.invalidPlansCount
            return true

        if not newState.hasConflict:
            planner.validPlans.add(newState)
            return true

        # Ignore conflicting plans if there is a valid plan
        if planner.validPlans.len == 0:
            planner.conflictingPlans.add(newState)

        return true

    # The plan is not finished, generate next possible plans

    # Valid plans
    let firstConflictIndex = planner.firstConflictIndexes[agentId + 1]
    for combinationId in 0..<firstConflictIndex.int:
        let nextPlan = PendingPlan(
            frozenState: newState,
            selectedCombination: combinationId.uint8,
        )
        planner.pendingValidPlans.add(nextPlan)
        inc planner.totalPlansCount

    # Optionally conflicting plans
    if planner.validPlans.len != 0:
        return true

    let totalCombinations = planner.combinationsPerAgent[agentId + 1].combinations.len
    
    # Full plans should be added to the valid queue to complete them asap
    if agentId.int + 1 == planner.agentNames.len:
        for combinationId in firstConflictIndex.int..<totalCombinations:
            let nextPlan = PendingPlan(
                frozenState: newState,
                selectedCombination: combinationId.uint8,
            )
            planner.pendingValidPlans.add(nextPlan)
            inc planner.totalPlansCount

        return true

    # Not going to produce completed plan, so add to conflicting queue
    for combinationId in firstConflictIndex.int..<totalCombinations:
        let nextPlan = PendingPlan(
            frozenState: newState,
            selectedCombination: combinationId.uint8,
        )
        planner.pendingConflictingPlans.add(nextPlan)
        inc planner.totalPlansCount
    
    return true


proc selectBest*(plans: seq[FrozenState], targetCount: int, scorer: proc(plan: FrozenState): int): seq[FrozenState] =
    var resultList = newSeq[FrozenState](0).toDeque()

    var minScore = int.high
    var maxScore = 0

    # Simply go through the list, score each plan, and maintain the minimum (targetCount)number of plans
    for plan in plans:
        let score = scorer(plan)
        if score <= minScore:
            minScore = score
            resultList.addFirst(plan)
            if resultList.len > targetCount:
                discard resultList.popLast()

        elif score > maxScore and resultList.len < targetCount:
            maxScore = score
            resultList.addLast(plan)

    return resultList.toSeq()


proc limitPlans*(planner: Planner, targetCount: int = 1000) =
    if planner.validPlans.len > targetCount:
        planner.validPlans.shuffle()
        planner.validPlans = planner.validPlans.selectBest(
            targetCount,
            (plan: FrozenState) => planner.scorer.calculatePlanDeviationScore(plan))
        if planner.conflictingPlans.len > 0:
            planner.conflictingPlans = @[]
        if planner.pendingConflictingPlans.len > 0:
            planner.pendingConflictingPlans = @[]

    if planner.conflictingPlans.len > targetCount:
        planner.conflictingPlans.shuffle()
        planner.conflictingPlans = planner.conflictingPlans.selectBest(
            targetCount,
            (plan: FrozenState) => planner.scorer.calculatePlanScore(plan))


proc resultingPlans*(planner: Planner): seq[FrozenState] =
    result = planner.validPlans
    if planner.validPlans.len != 0: return
    result &= planner.conflictingPlans