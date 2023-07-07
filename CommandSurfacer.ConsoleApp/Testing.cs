using System.Collections;

namespace CommandSurfacer.ConsoleApp;

internal static class Testing
{
    public static void RunMain(string[] args)
    {
        WriteToConsole("INPUT", args);

        args = CorrectBrokenValues(args);
        WriteToConsole("OUTPUT", args);

        return;

        var array = new string[]
        {
            "test",
            "\"D:\\", " test.txt\"",
            "'D:\\", " test.txt'"
        };

        WriteToConsole("INPUT", array);

        var output = CorrectBrokenValues(array);
        WriteToConsole("OUTPUT", output);

        return;
    }

    private static void WriteToConsole(string message, string[] input)
    {
        Console.WriteLine(message);

        foreach (var str in input)
            Console.WriteLine(str);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();
    }

    private static void Log(string message, params object[] args) => Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss.ffffff") + " " + message, args);

    private static string[] CorrectBrokenValues(string[] input)
    {
        var result = new List<string>();

        var allowedWrapStrings = new char[] { '"', '\'' };

        var wrap = default(char?);
        var open = default(bool);

        var correctedValue = string.Empty;
        for (var i = 0; i < input.Length; i++)
        {
            var currentValue = input[i];
            Log("Current value {0}", currentValue);

            if (StartsWithAny(currentValue, allowedWrapStrings, out var startsWith))
            {
                Log("Starts with {0}", startsWith);

                open = true;

                wrap = startsWith;
                currentValue = currentValue.TrimStart(startsWith);

                correctedValue += currentValue;

                if (EndsWithAny(correctedValue, allowedWrapStrings, out var endsWith) && wrap == endsWith)
                {
                    Log("Ends with {0}", endsWith);

                    open = false;

                    wrap = default(char?);
                    correctedValue = correctedValue.TrimEnd(endsWith);

                    result.Add(correctedValue);
                    correctedValue = string.Empty;
                }
            }
            else if (open)
            {
                if (EndsWithAny(currentValue, allowedWrapStrings, out var endsWith) && wrap == endsWith)
                {
                    Log("Ends with {0}", endsWith);

                    open = false;

                    wrap = default(char?);
                    currentValue = currentValue.TrimEnd(endsWith);

                    correctedValue += currentValue;

                    result.Add(correctedValue);
                    correctedValue = string.Empty;
                }
                else
                {
                    Log("Middle");
                    correctedValue += currentValue;
                }
            }
            else
            {
                result.Add(currentValue);
            }


            //if (StartsWithAny(currentValue, allowedWrapStrings, out var startsWith))
            //{
            //    open = true;

            //    wrap = startsWith;
            //    currentValue = currentValue.TrimStart(startsWith);

            //    correctedValue += currentValue;
            //}


            //if (open && EndsWithAny(currentValue, allowedWrapStrings, out var endsWith) && wrap == endsWith)
            //{
            //    open = false;

            //    wrap = default(char?);
            //    currentValue = currentValue.TrimEnd(endsWith);

            //    correctedValue += currentValue;

            //    result.Add(correctedValue);
            //    correctedValue = string.Empty;
            //}
            //else
            //{
            //    open = false;

            //    correctedValue += currentValue;

            //    result.Add(correctedValue);
            //    correctedValue = string.Empty;
            //}
        }

        return result.ToArray();
    }

    private static bool StartsWithAny(string input, char[] search, out char found) => SearchString(input, search, out found, (s, c) => s.StartsWith(c));
    private static bool EndsWithAny(string input, char[] search, out char found) => SearchString(input, search, out found, (s, c) => s.EndsWith(c));
    private static bool Contains(string input, char[] search, out char found) => SearchString(input, search, out found, (s, c) => s.Contains(c));

    private static bool SearchString(string input, char[] search, out char found, Func<string, char, bool> evaluation)
    {
        for (var i = 0; i < search.Length; i++)
        {
            var currentSearchFor = search[i];

            if (evaluation(input, currentSearchFor))
            {
                found = currentSearchFor;
                return true;
            }
        }

        found = default(char);
        return false;
    }


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

    public class Arguments : IEnumerable<string>
    {
        public IEnumerable<string> Keys => Dictionary.Keys;
        public IEnumerable<string> Values => List.Concat(Dictionary.SelectMany(kvp => kvp.Value));


        public List<string> List { get; set; }

        public Dictionary<string, List<string>> Dictionary { get; set; }


        public IEnumerator<string> GetEnumerator() => Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
    }

    private static bool ContainsAny(string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.Contains(option, StringComparison.OrdinalIgnoreCase));

    private static bool StartsWithAny(string input, string[] options, out string found) =>
        AnyStringsOperation(input, options, out found, (str, option) => str.StartsWith(option, StringComparison.OrdinalIgnoreCase));

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

    private static string[] ParseValues(string[] array, Type targetType, SurfaceAttribute surfaceAttribute)
    {
        var prefixes = new string[] { "--", "-", "\\" };

        var sub = ArrayUtils.Slice(array,
            item => ArrayUtils.Cartesian(prefixes, surfaceAttribute.Name).Contains(item, StringComparer.OrdinalIgnoreCase),
            item => prefixes.Any(prefix => item.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)));

        return sub;
    }
}

/*
 * 
        var currentProcess = Process.GetCurrentProcess();
        var output = processService.RunProcess("powershell.exe", $"-Command {{ Get-CimInstance Win32_Process -Filter \"ProcessId = '{currentProcess.Id}'\" | select ParentProcessId }}");
 */



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