namespace LeadCalendar.Models;

public struct Combination<T>
{
    public T[] Weeks { get; init; }
    
    public Combination(T[] weeks)
    {
        Weeks = weeks;
    }
}