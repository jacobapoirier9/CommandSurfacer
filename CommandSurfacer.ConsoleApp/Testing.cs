using CommandSurfacer.Services;
using System.Text;

namespace CommandSurfacer.ConsoleApp;

internal static class Testing
{
    private static void Log(string message, params object[] args) => Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffffff") + " " + message, args);
    public static void RunMain(string[] args)
    {
    }

    private static bool ContainsAny(this string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.Contains(option, StringComparison.OrdinalIgnoreCase));

    private static bool StartsWithAny(this string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.StartsWith(option, StringComparison.OrdinalIgnoreCase));

    private static bool EndsWithAny(this string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.EndsWith(option, StringComparison.OrdinalIgnoreCase));

    private static bool AnyStringsOperation(string input, string[] options, out string found, Func<string, string, bool> condition)
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


    private static List<string> ParseCommandLine(string input)
    {
        var list = new List<string>();

        // Step 1
        if (string.IsNullOrWhiteSpace(input))
            return list;

        var current = new StringBuilder();
        var quotingCharacter = default(char?);

        // Step 2
        for (var i = 0; i < input.Length; i++)
        {
            var character = input[i];

            if (new char[] { '"', '\'' }.Contains(character))
            {
                if (quotingCharacter.HasValue)
                {
                    list.Add(current.ToString());
                    current.Clear();
                    quotingCharacter = default(char?);
                }
                else
                {
                    quotingCharacter = character;
                }
            }
            else if (character == ' ')
            {
                if (quotingCharacter.HasValue)
                {
                    current.Append(character);
                }
                else if (current.Length > 0)
                {
                    list.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(character);
            }
        }

        // Step 3
        if (current.Length > 0)
            list.Add(current.ToString());

        return list;
    }

    private static Arguments ParseArguments(string input) => ParseArguments(ParseCommandLine(input).ToArray());
    private static Arguments ParseArguments(string[] array)
    {
        var list = new List<string>();
        var dictionary = new Dictionary<string, List<string>>();

        var prefixes = new string[] { "--", "-", "/" };
        var separators = new string[] { "=", ":" };
        var separatedValueTrims = new char[] { '"', '\'', ',' };

        var currentKey = default(string);
        for (var i = 0; i < array.Length; i++)
        {
            var item = array[i];

            if (StartsWithAny(item, prefixes, out var prefix))
            {
                currentKey = item.Replace(prefix, string.Empty);

                if (ContainsAny(currentKey, separators, out var separator))
                {
                    var split = currentKey.Split(separator);

                    currentKey = split[0];
                    dictionary.Add(currentKey, new List<string>() { split[1].Trim(separatedValueTrims) });
                }
                else if (!dictionary.ContainsKey(currentKey))
                    dictionary.Add(currentKey, new List<string>());
            }
            else if (currentKey is null)
                list.Add(item);
            else
                dictionary[currentKey].Add(item);
        }

        return new Arguments
        {
            List = list,
            Dictionary = dictionary
        };
    }

}

internal static class ArrayUtils
{
    internal static T[] Slice<T>(T[] array, Func<T, bool> firstAndSecondPredicate) => Slice(array, firstAndSecondPredicate, firstAndSecondPredicate);
    internal static T[] Slice<T>(T[] array, Func<T, bool> firstPredicate, Func<T, bool> secondPredicate)
    {
        var firstIndex = default(int?);
        var secondIndex = default(int?);

        for (var i = 0; i < array.Length; i++)
        {
            var item = array[i];

            if (firstIndex is null)
            {
                if (firstPredicate(item))
                    firstIndex = i;
            }
            else if (secondIndex is null)
            {
                if (secondPredicate(item))
                    secondIndex = i;
            }
            else
                return Slice(array, firstIndex.Value, secondIndex.Value);
        }

        return Slice(array, firstIndex ?? 0, secondIndex ?? array.Length);
    }

    internal static T[] Slice<T>(T[] array, int startIndex, int endIndex)
    {
        var innerArray = new T[endIndex - startIndex];
        for (var i = 0; i < innerArray.Length; i++)
        {
            var item = array[startIndex + i];
            innerArray[i] = item;

            array[startIndex + i] = default(T); // TODO: Remove this when LINQ has been removed from outerArray
        }

        // TODO: Do not use LINQ to calculate outerArray.
        var outerArray = array.Where(item => !object.Equals(item, default(T))).ToArray();
        array = outerArray;

        return innerArray;
    }

    internal static string[] Cartesian(this string[] left, params string[] right)
    {
        var array = (from l in left
                     join r in right
                     on 1 equals 1
                     select l + r).ToArray();

        return array;
    }

    internal static void DumpArray<T>(string message, T[] array)
    {
        Console.WriteLine(message);
        foreach (var item in array)
        {
            Console.WriteLine(item);
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
    }
}