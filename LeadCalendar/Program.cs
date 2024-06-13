// See https://aka.ms/new-console-template for more information

using LeadCalendar.Models;

/*
  | An | Bo | Ca | Da | Ev | Fr | Ge | Ha
0 | ❔ | ❔ | ❔ | ✅ | ❔ | ❔ | ✅ | ✅
--|----|----|----|----|----|----|----
1 | ❌ | ❔ | ❌ | ❔ | ❌ | ❔ | ❔ | ❔
2 | ❔ | ❔ | ❔ | ❔ | ❌ | ❔ | ❔ | ❔
3 | ❌ | ❔ | ❔ | ❔ | ❔ | ❔ | ❔ | ❌
4 | ❔ | ❔ | ❔ | ❔ | ❔ | ❔ | ❔ | ❌
5 | ❔ | ❔ | ❔ | ❔ | ❔ | ❌ | ❔ | ❔
 */
var planner = new PlannerBuilder(5, 2)
    .AddAgent("Anne", 1, 3)
    .AddAgent("Bob")
    .AddAgent("Carol", 1)
    .AddAgent("David")
    .AddAgent("Eve", 2, 1)
    .AddAgent("Frank", 5)
    .AddAgent("George")
    .AddAgent("Harry", 3, 4, 5)
    .SetPreviousWeek("David", "George", "Harry")
    .Build();
    
Console.WriteLine("Initial plan:");
//Console.Write(planner.GetCurrentPlan().PresentAsTable(planner));
Console.WriteLine();

planner.CheckHasUnavoidableConflicts();