namespace CommandSurfacer.ConsoleApp;

internal static class Utils
{
    internal static object[] CombineArrays(object[] left, params object[] right)
    {
        var combined = new object[left.Length + right.Length];

        for (var i = 0; i < left.Length; i++)
            combined[i] = left[i];

        for (var i = 0; i < right.Length; i++)
            combined[i + left.Length] = right[i];

        return combined;
    }

}