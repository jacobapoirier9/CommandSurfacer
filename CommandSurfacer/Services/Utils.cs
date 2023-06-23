namespace CommandSurfacer.Services;

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

    internal static string ReplaceFirstOccurance(string input, string substring, string replacement)
    {
        var firstIndex = input.IndexOf(substring);

        var stringStart = input.Substring(0, firstIndex);
        var stringEnd = input.Substring(firstIndex + substring.Length);

        var stringFinal = stringStart + replacement + stringEnd;
        return stringFinal;
    }

    public static bool IsTrue(this bool? value) => value.HasValue && value.Value;
}