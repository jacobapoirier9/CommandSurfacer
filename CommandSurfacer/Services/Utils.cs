using Microsoft.Extensions.DependencyInjection;
using System.Text;

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


    internal static bool ContainsAny(this string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.Contains(option, StringComparison.OrdinalIgnoreCase));

    internal static bool StartsWithAny(this string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.StartsWith(option, StringComparison.OrdinalIgnoreCase));

    internal static bool EndsWithAny(this string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.EndsWith(option, StringComparison.OrdinalIgnoreCase));

    internal static bool AnyStringsOperation(string input, string[] options, out string found, Func<string, string, bool> condition)
    {
        for (var i = 0; i < options.Length; i++)
        {
            var option = options[i];

            if (condition.Invoke(input, option))
            {
                found = option;
                return true;
            }
        }

        found = null;
        return false;
    }

    internal static bool TryGetService<T>(this IServiceProvider serviceProvider, out T service)
    {
        service = serviceProvider.GetService<T>();
        return service is not null;
    }

    internal static string PowerShellEncodeCommand(this string input)
    {
        var bytes = Encoding.Unicode.GetBytes(input);
        var encoded = Convert.ToBase64String(bytes);

        return "-EncodedCommand " + encoded;
    }
}