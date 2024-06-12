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
    .AddAgent(new Agent("Anne", 1, 3))
    .AddAgent(new Agent("Bob"))
    .AddAgent(new Agent("Carol", 1))
    .AddAgent(new Agent("David"))
    .AddAgent(new Agent("Eve", 2, 1))
    .AddAgent(new Agent("Frank", 5))
    .AddAgent(new Agent("George"))
    .AddAgent(new Agent("Harry", 3, 4, 5))
    .SetPreviousWeek("David", "George", "Harry")
    .Build();
    
Console.WriteLine("Initial plan:");
Console.Write(planner.GetCurrentPlan().PresentAsTable(planner));
Console.WriteLine();

planner.CheckHasUnavoidableConflicts();