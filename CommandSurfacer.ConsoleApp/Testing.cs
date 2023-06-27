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

            if (StartsWith(currentValue, allowedWrapStrings, out var startsWith))
            {
                open = true;

                wrap = startsWith;
                currentValue = currentValue.TrimStart(startsWith);

                correctedValue += currentValue;
            }
            else if (open && EndsWith(currentValue, allowedWrapStrings, out var endsWith) && wrap == endsWith)
            {
                open = false;

                wrap = default(char?);
                currentValue = currentValue.TrimEnd(endsWith);

                correctedValue += currentValue;

                result.Add(correctedValue);
                correctedValue = string.Empty;
            }
            else
            {
                open = false;

                result.Add(currentValue);
                correctedValue = string.Empty;
            }
        }

        return result.ToArray();
    }

    private static bool StartsWith(string input, char[] search, out char found) => SearchString(input, search, out found, (s, c) => s.StartsWith(c));
    private static bool EndsWith(string input, char[] search, out char found) => SearchString(input, search, out found, (s, c) => s.EndsWith(c));
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
}

/*
 * 
        var currentProcess = Process.GetCurrentProcess();
        var output = processService.RunProcess("powershell.exe", $"-Command {{ Get-CimInstance Win32_Process -Filter \"ProcessId = '{currentProcess.Id}'\" | select ParentProcessId }}");
 */