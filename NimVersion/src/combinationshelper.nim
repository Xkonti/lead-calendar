func calcCombinations*[T](items: openArray[T], length: uint8): seq[seq[T]] =
  ## Calculates all possible combinations of items with a specified length.
  ## Example:
  ##   calcCombinations([1, 2, 3], 2)
  ##   # => @[[1, 2], [1, 3], [2, 3]]
  var combinations: seq[seq[T]] = @[]
  let items = @items
  proc generate(start: int, combination: seq[T]): void =
    for i in start..<items.len:
      var newCombination = combination
      newCombination.add items[i]
      if (newCombination.len == length.int):
        combinations.add newCombination
      if newCombination.len < length.int:
        generate(i + 1, newCombination)

  generate(0, @[])
  return combinations