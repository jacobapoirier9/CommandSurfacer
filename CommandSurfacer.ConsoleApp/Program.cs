using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommandSurfacer.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    private static async Task MainAsync(string[] args)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            args = new string[] { "enter-test" };
        }

        var client = Client.Create()
            .AddInteractiveConsole(options =>
            {
                options.Banner = "Welcome to Jake's custom CLI";
            })
            .AddConsoleHelp()
            .AddServices(services =>
            {
                services.AddSingleton<TestService>();
                services.AddSingleton<AppCmdService>();
            });

        await client.RunAsync(args);
    }
}

public class AppCmdService
{
    private static string _appCmd = @$"{Environment.GetEnvironmentVariable("WinDir")}\System32\inetsrv\appcmd.exe";

    private readonly IProcessService _processService;
    public AppCmdService(IProcessService processService)
    {
        _processService = processService;
    }

    public async Task<string> GetConfiguration(string configSection)
    {
        var command = string.Format("list config -section:{0}", configSection);
        var result = await _processService.RunAsync(_appCmd, command);

        return result.StandardOutputString;
    }
}

public class TestService
{
    [Surface("enter-test")]
    public async Task EnterTestAsync(AppCmdService service, string[] items)
    {
        foreach (var item in items)
            Console.WriteLine(item);

        var xml = await service.GetConfiguration("system.applicationHost/log");

        Console.WriteLine(xml);
    }
}