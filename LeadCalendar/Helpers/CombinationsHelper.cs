using LeadCalendar.Models;

namespace LeadCalendar.Helpers;

public static class CombinationsHelper
{
    /*
func calcCombinations*[T](items: openArray[T], length: int): seq[seq[T]] =
  ## Calculates all possible combinations of items with a specified length.
  ## Example:
  ##   calcCombinations([1, 2, 3], 2)
  ##   # => @[[1, 2], [1, 3], [2, 3]]
  var combinations: seq[seq[T]] = @[]
  let items = items.toSeq()
  proc generate(start: int, combination: seq[T]): void =
    for i in start..<items.len:
      var newCombination = combination
      newCombination.add items[i]
      if (newCombination.len == length):
        combinations.add newCombination
      if newCombination.len < length:
        generate(i + 1, newCombination)

  generate(0, @[])
  return combinations
     */
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