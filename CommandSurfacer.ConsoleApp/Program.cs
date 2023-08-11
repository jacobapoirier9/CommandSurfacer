using CommandSurfacer.Models;
using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using static CommandSurfacer.ConsoleApp.Testing;

namespace CommandSurfacer.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    private static async Task MainAsync(string[] args)
    {
        DumpTypeInformation(typeof(int[]));
        DumpTypeInformation(typeof(string[]));
        DumpTypeInformation(typeof(object[]));

        //return;

        if (System.Diagnostics.Debugger.IsAttached)
            args = new string[] { "test --names Jake, JJ, Kam --notused" };

        var client = Client.Create()
            .AddInteractiveConsole(options =>
            {
                options.Banner = "Welcome to Jake's custom CLI";
            })
            .AddConsoleHelp()
            .AddServices(services =>
            {
                services.AddSingleton<TestService>();
                services.AddSingleton<Test2Service>();
            });

        await client.RunAsync(args);
    }

    private static void DumpTypeInformation(Type targetType)
    {
        Console.WriteLine("Breakdown of {0}", targetType);
        Console.WriteLine("IEnumerable<object>: {0}", targetType.IsAssignableTo(typeof(IEnumerable<object>)));
        Console.WriteLine("IEnumerable<>: {0}", targetType.IsAssignableTo(typeof(IEnumerable<>)));
        Console.WriteLine("IEnumerable: {0}", targetType.IsAssignableTo(typeof(IEnumerable)));
        Console.WriteLine();

        foreach (var @interface in targetType.GetInterfaces())
            Console.WriteLine(@interface);


        Console.WriteLine();
        Console.WriteLine();

    }
}

[Group("test-group")]
public class TestService
{
    [Surface("test")]
    public void Test(List<string> names)
    {
        foreach (var name in names)
            Console.WriteLine(names);
    }
}

[Group("test-2-group", HelpText = "This is how you do it")]
public class Test2Service
{
    [Surface("test2")]
    public void Test()
    {

    }
}

/*
 * 
        var currentProcess = Process.GetCurrentProcess();
        var output = processService.RunProcess("powershell.exe", $"-Command {{ Get-CimInstance Win32_Process -Filter \"ProcessId = '{currentProcess.Id}'\" | select ParentProcessId }}");
 */