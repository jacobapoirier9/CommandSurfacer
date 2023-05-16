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
        args = new string[] { "test --test" };

        var client = Client.Create()
            .AddInteractiveConsole(options =>
            {
                options.Banner = "Welcome to Jake's custom CLI";
            })
            .AddConsoleHelpMenu()
            .AddServices(services =>
            {
                services.AddSingleton<TestService>();
            });

        client.Run(args);


    }
}


public class TestService
{
    [Surface("test")]
    public async Task Test(IProcessService processService, bool test)
    {
        var currentProcess = Process.GetCurrentProcess();
        var output = processService.Run("powershell.exe", $"-Command {{ Get-CimInstance Win32_Process -Filter \"ProcessId = '{currentProcess.Id}'\" | select ParentProcessId }}");
    }
}
