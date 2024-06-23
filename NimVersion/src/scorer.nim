import ./statecombination
import ./frozenstate

const conflictCountMultiplier = 10000
const deviationMultiplier = 200
const deviationAvgMultiplier = 100


proc scoreCombination(combination: StateCombination): int =
    if not combination.hasConflict: return 0
    var conflictCount = 0
    var isPreviousSelected = combination.weekSelections[0]
    for week in 1..<combination.weekSelections.len:
        let currentSelected = combination.weekSelections[week]
        if isPreviousSelected and currentSelected: conflictCount += 1
        isPreviousSelected = currentSelected
    
    return conflictCount * conflictCountMultiplier


proc calculateCombinationScores(combinationsPerAgent: seq[AgentStateCombinations]): seq[seq[int]] =
    result = newSeq[seq[int]](combinationsPerAgent.len)
    for agentId in 0..<combinationsPerAgent.len:
        let combinationsCount = combinationsPerAgent[agentId].combinations.len
        result[agentId] = newSeq[int](combinationsCount) 
        for combinationId in 0..<combinationsCount:
            result[agentId][combinationId] = combinationsPerAgent[agentId].combinations[combinationId].scoreCombination()


type
    Scorer* = ref object
        combinationScoresPerAgent: seq[seq[int]] = @[]


proc newScorer*(combinationsPerAgent: seq[AgentStateCombinations]): Scorer =
    return Scorer(
        combinationScoresPerAgent: calculateCombinationScores(combinationsPerAgent)
    )


proc getCombinationScore(scorer: Scorer, agentIndex: int, combinationId: int): int =
    return scorer.combinationScoresPerAgent[agentIndex][combinationId]


proc calculatePlanConflictScore*(scorer: Scorer, plan: FrozenState): int =
    let statIds = plan.getStatCombinationIds()
    for agentId in 0..<statIds.len:
        result += scorer.getCombinationScore(agentId, statIds[agentId].int)


proc calculatePlanDeviationScore*(scorer: Scorer, plan: FrozenState): int =
    var minAgentsPerWeek = int.high
    var maxAgentsPerWeek = 0
    var sum = 0
    for weekId in 0..<plan.selectionsPerWeek.len:
        let selected = plan.selectionsPerWeek[weekId].int
        sum += selected
        if selected < minAgentsPerWeek: minAgentsPerWeek = selected
        if selected > maxAgentsPerWeek: maxAgentsPerWeek = selected

    let deviation = maxAgentsPerWeek - minAgentsPerWeek
    let deviationScore = deviation * deviationMultiplier
    
    let avg = sum / plan.selectionsPerWeek.len
    let deviationDown = maxAgentsPerWeek.float - avg
    let deviationUp = avg - minAgentsPerWeek.float
    let maxDeviation = max(deviationDown, deviationUp)
    let deviationAvgScore = maxDeviation * deviationAvgMultiplier
    return deviationScore + deviationAvgScore.int


proc calculatePlanScore*(scorer: Scorer, plan: FrozenState): int =
    let conflictScore = scorer.calculatePlanConflictScore(plan)
    let deviationScore = scorer.calculatePlanDeviationScore(plan)
    # TODO: Add a penalty for multiple weeks with similar agents
    return conflictScore + deviationScore