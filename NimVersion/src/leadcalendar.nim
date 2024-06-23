import std/[math, random, stats, sugar, times]
import ./plannerbuilder
import ./frozenstate
import ./planner
import ./scorer

let thePlanner = newPlannerBuilder(5, 2, 2)
    .addAgent("Anne", 1, 3)
    .addAgent("Bob")
    .addAgent("Carol", 1)
    .addAgent("David")
    .addAgent("Eve", 2, 1)
    .addAgent("Frank", 5)
    .addAgent("George")
    .addAgent("Harry", 3, 4, 5) # Intentional conflict
    .addAgent("Iris")
    .addAgent("John")
    .addAgent("Karin")
    .setPreviousWeek("David", "George", "Harry")
    .build()

const iterationsPerMsg = 1_000_000
var measurements: seq[float] = @[]
var start: float

var iteration = 0
var hasFinished = false

while not hasFinished:

    if iteration mod iterationsPerMsg == 0:
         start = cpuTime()

    hasFinished = not thePlanner.findPlansLoopIteration()

    inc iteration
    if iteration mod iterationsPerMsg == 0:
        # thePlanner.limitPlans()

        let elapsed = cpuTime() - start
        measurements.add elapsed * 1000.0

        echo "Iteration: ", iteration
        thePlanner.printStats()
    


echo "Finished after ", iteration, " iterations"
thePlanner.printStats()


let avg = measurements.mean()
echo "Average time per ", iterationsPerMsg, " iterations: ", avg, " ms"

let total = measurements.sum()
echo "Total time: ", total, " ms"
let iterationAvg = total / iteration.float
echo "Average iteration time: ", iterationAvg * 1000000.0, " ns"



echo "\n\n ==================="
echo "====  SCORING  ===="
echo "===================\n\n"

var resultingPlans = thePlanner.resultingPlans()
resultingPlans.shuffle()
let bestPlans = resultingPlans.selectBest(10, (plan: FrozenState) => thePlanner.scorer.calculatePlanScore(plan))

var planIndex = 0
var worstScore = 0
for plan in bestPlans:
    let planScore = thePlanner.scorer.calculatePlanScore(plan)
    if planScore > worstScore: worstScore = planScore
    planIndex += 1
    echo "Plan #", planIndex, " (", worstScore - thePlanner.scorer.calculatePlanScore(plan), "pts)"
    echo plan.presentAsList(thePlanner.agentNames, thePlanner.combinationsPerAgent)
    echo ""