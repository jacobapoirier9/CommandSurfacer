using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CommandSurfacer.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    private static async Task MainAsync(string[] args)
    {
        args = new string[] { "help" };

        var client = Client.Create()
            .AddInteractiveConsole(options =>
            {
                options.Banner = "Welcome to Jake's custom CLI";
            })
            .AddConsoleHelpMenu()
            .AddServices(services =>
            {
                services.AddSingleton<TestService>();
                services.AddSingleton<ServiceOne>();
            });

        client.Run(args);


    }
}


public class TestService
{
    [Surface("test", HelpText = "Fire the test method, generally used for debugging purposes")]
    public async Task Test(string path)
    {
    }
}

[Surface("service-one")]
public class ServiceOne
{
    [Surface("one", HelpText = "Run the first method")]
    public void TestOne(int age) { }
}

/*
 * 
        var currentProcess = Process.GetCurrentProcess();
        var output = processService.Run("powershell.exe", $"-Command {{ Get-CimInstance Win32_Process -Filter \"ProcessId = '{currentProcess.Id}'\" | select ParentProcessId }}");
 */