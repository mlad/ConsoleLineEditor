namespace ConsoleLineEditor.Internal.Extensions;

internal static class IntExtensions
{
    public static int WrapAround(this int value, int min, int max)
    {
        if (value < min)
        {
            return max;
        }

        if (value > max)
        {
            return min;
        }

        return value;
    }
}