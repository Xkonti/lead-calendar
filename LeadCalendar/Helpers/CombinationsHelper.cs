using LeadCalendar.Models;

namespace LeadCalendar.Helpers;

public static class CombinationsHelper
{
    public static IEnumerable<Combination<T>> GenerateCombinations<T>(T[] items, int targetLength)
    {
        var combinations = new List<Combination<T>>();
        Generate(0, []);
        return combinations;

        void Generate(int start, T[] combination)
        {
            for (var i = start; i < items.Length; i++)
            {
                var newCombination = new T[combination.Length + 1];
                Array.Copy(combination, newCombination, combination.Length);
                newCombination[combination.Length] = items[i];
                if (newCombination.Length == targetLength)
                    combinations.Add(new Combination<T>(newCombination));
                if (newCombination.Length < targetLength)
                    Generate(i + 1, newCombination);
            }
        }
    }
}