namespace UnitTests.Helpers;

/// <summary>
/// A <see cref="Random"/> subclass that returns a pre-configured sequence of values,
/// allowing deterministic testing of code that depends on random number generation.
/// </summary>
public class ControlledRandom(params int[] values) : Random
{
    private readonly Queue<int> _values = new(values);

    public override int Next(int minValue, int maxValue) => _values.Dequeue();
}
