using System.Text;

namespace LeadCalendar.Models;

// public record FormedPlan
// {
//     public AgentState[] AgentStates { get; init; }
//     
//     public string PresentAsTable(Planner planner)
//     {
// /*
//   ## Presents plan as a table
//   ## Example:
//   ##   | A  | B  | C  | D 
//   ## 0 | ❌ | ✅ | ✅ | ❌
//   ## --|----|----|----|----
//   ## 1 | ❌ | ❌ | ✅ | ✅
//   ## 2 | ❔ | ❌ | ✅ | ❌
//   ## 3 | ❌ | ✅ | ❌ | ✅
//   ## 4 | ✅ | ❔ | ❌ | ❌
//  */
//
//         var sb = new StringBuilder();
//         var agentNames = planner.Agents.Select(a => a.Name).ToArray();
//         
//         // Header
//         sb.Append(' ');
//         foreach (var agentName in agentNames)
//         {
//             sb.Append($" | {agentName[..2]}");
//         }
//         sb.AppendLine();
//         
//         // Week 0
//         AppendWeek(0);
//         
//         // Divider
//
//         sb.Append("--");
//         for (var i = 1; i < agentNames.Length; i++)
//         {
//             sb.Append("|----");
//         }
//         sb.AppendLine();
//         
//         for (var weekIndex = 1; weekIndex <= planner.WeeksCount; weekIndex++)
//         {
//             AppendWeek(weekIndex);
//         }
//         
//         return sb.ToString();
//         
//         void AppendWeek(int weekIndex)
//         {
//             sb.Append(weekIndex);
//             for (var agentIndex = 0; agentIndex < planner.Agents.Length; agentIndex++)
//             {
//                 var stateIcon = AgentStates[agentIndex].IsSelected(weekIndex)
//                     ? "✅"
//                     : AgentStates[agentIndex].IsEmpty(weekIndex, planner)
//                         ? "❔"
//                         : "❌";
//                 sb.Append($" | {stateIcon}");
//             }
//             sb.AppendLine();
//         }
//     }
// }