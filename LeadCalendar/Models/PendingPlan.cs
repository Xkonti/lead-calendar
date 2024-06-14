namespace LeadCalendar.Models;

/// <summary>
/// A plan that is not yet complete. Used to describe plan variants.
/// </summary>
public struct PendingPlan
{
    /// <summary>
    /// The part of the plan that already established.
    /// This is a reference that is shared between various plan variants.
    /// </summary>
    public FrozenState FrozenState { get; init; }
    
    /// <summary>
    /// The state combination that is expected to be added to the plan.
    /// </summary>
    public byte SelectedCombination { get; init; }
}