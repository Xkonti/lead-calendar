namespace LeadCalendar.Helpers;

public static class ArrayExtensions
{
    /// <summary>
    /// Appends a new item to the end of an array in a performant way using Array.Copy.
    /// </summary>
    /// <returns>New array with the new item appended.</returns>
    public static T[] Append<T>(this T[] array, T newElement)
    {
        // Create a new array that is one item longer than the original
        var newArray = new T[array.Length + 1];
        
        // Copy the elements from the original array to the new array
        Array.Copy(array, newArray, array.Length);
        
        // Add the new item at the end of the new array
        newArray[array.Length] = newElement;
        
        return newArray;
    }

    /// <summary>
    /// Increments each week counter by one if the corresponding boolean from week selections is true.
    /// The week selections array includes the week 0, so it needs to be skipped.
    /// </summary>
    /// <returns>The new array with the incremented values.</returns>
    /// <exception cref="ArgumentException">When the arrays are not the same length.</exception>
    public static byte[] Increment(this byte[] array, bool[] other)
    {
        if (array.Length != other.Length - 1)
            throw new ArgumentException("Arrays must have the same length");
        
        var newArray = (byte[])array.Clone();
        for (var i = 0; i < array.Length; i++)
            if (other[i+1]) newArray[i]++;
        
        return newArray;
    }
    
    public static T[] Reverse<T>(this T[] array) {
        for (var i = 0; i < array.Length / 2; i++) {
            var j = array.Length - 1 - i;
            (array[i], array[j]) = (array[j], array[i]);
        }
        return array;
    }
}