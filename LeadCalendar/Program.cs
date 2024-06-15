// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using LeadCalendar;
using LeadCalendar.Helpers;

/*
Benchmark:
  | An | Bo | Ca | Da | Ev | Fr | Ge | Ha
0 | ❔ | ❔ | ❔ | ✅ | ❔ | ❔ | ✅ | ✅
--|----|----|----|----|----|----|----
1 | ❌ | ❔ | ❌ | ❔ | ❌ | ❔ | ❔ | ❔
2 | ❔ | ❔ | ❔ | ❔ | ❌ | ❔ | ❔ | ❔
3 | ❌ | ❔ | ❔ | ❔ | ❔ | ❔ | ❔ | ❌
4 | ❔ | ❔ | ❔ | ❔ | ❔ | ❔ | ❔ | ❌
5 | ❔ | ❔ | ❔ | ❔ | ❔ | ❌ | ❔ | ❌

var planner = new PlannerBuilder(5, 2, 2)
    .AddAgent("Anne", 1, 3)
    .AddAgent("Bob")
    .AddAgent("Carol", 1)
    .AddAgent("David")
    .AddAgent("Eve", 2, 1)
    .AddAgent("Frank", 5)
    .AddAgent("George")
    .AddAgent("Harry", 3, 4, 5) // Intentional conflict
    .AddAgent("Iris")
    .AddAgent("John")
    .AddAgent("Karin")
    .SetPreviousWeek("David", "George", "Harry")
    .Build();
 */

var planner = new PlannerBuilder(5, 2, 2)
    .AddAgent("Anne", 1, 3)
    .AddAgent("Bob")
    .AddAgent("Carol", 1)
    .AddAgent("David")
    .AddAgent("Eve", 2, 1)
    .AddAgent("Frank", 5)
    .AddAgent("George")
    .AddAgent("Harry", 3, 4, 5) // Intentional conflict
    .AddAgent("Iris")
    .AddAgent("John")
    .AddAgent("Karin")
    .SetPreviousWeek("David", "George", "Harry")
    .Build();

planner.CheckHasUnavoidableConflicts();

const int iterationsPerMsg = 1000000;
var sw = new Stopwatch();
var measurements = new List<double>();

var iteration = 0;
var hasFinished = false;
while (!hasFinished)
{
    // Start stopwatch
    if (iteration % iterationsPerMsg == 0)
    {
        sw.Restart();
        planner.LimitPlans();
    }
    
    hasFinished = !planner.FindPlansLoopIteration();
    
    
    
    iteration++;
    if (iteration % iterationsPerMsg == 0)
    {
        sw.Stop();
        measurements.Add(sw.Elapsed.TotalMilliseconds);
        Console.WriteLine("Iteration: {0}", iteration);
        planner.PrintStats();
    }
}

sw.Stop();
measurements.Add(sw.Elapsed.TotalMilliseconds);

Console.WriteLine("\n\nFinished after {0} iterations", iteration);
planner.PrintStats();

var avg = measurements.Average();
Console.WriteLine($"Average time per {iterationsPerMsg:0.00} iterations: {avg} ms");

var total = measurements.Sum();
Console.WriteLine($"Total time: {total:0.00} ms");
var stdDev = Math.Sqrt(measurements.Average(x => Math.Pow(x - avg, 2)));
Console.WriteLine($"Standard deviation: {stdDev:0.00} ms");
var stdDevPercent = stdDev / avg * 100;
Console.WriteLine($"Standard deviation percent: {stdDevPercent:0.00}%");
var iterationAvg = total / iteration;
Console.WriteLine($"Average iteration time: {(iterationAvg * 1000000):0.00} ns");


Console.WriteLine("\n\n ===================");
Console.WriteLine("====  SCORING  ====");
Console.WriteLine("===================\n\n");

var resultingPlans = planner.ResultingPlans;
resultingPlans.Shuffle(); // Randomize the order so that we get some variety in results
var bestPlans = resultingPlans.SelectBest(10, planner.Scorer.CalculatePlanScore);

var planIndex = 0;
var worstScore = 0;
foreach (var plan in bestPlans)
{
    var planScore = planner.Scorer.CalculatePlanScore(plan);
    if (planScore > worstScore) worstScore = planScore;
    planIndex++;
    Console.WriteLine($"Plan #{planIndex} ({worstScore - planner.Scorer.CalculatePlanScore(plan)}pts)");
    Console.WriteLine(plan.PresentAsList(planner.AgentNames, planner.CombinationsPerAgent));
    Console.WriteLine();
}