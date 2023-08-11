using CommandSurfacer.Models;
using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
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
}

[Group("test-group")]
public class TestService
{
    [Surface("test")]
    public void Test(string[] names)
    {

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