using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace CommandSurfacer.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    private static async Task MainAsync(string[] args)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            args = new string[] { "enter-test --name 'Jake'" };
            args = new string[] { };
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

public class TestService
{
    public string Name { get; set; }

    [Surface("enter-test")]
    public async Task EnterTestAsync(IProcessService service, TestService request)
    {
        var parentProcess = await service.GetParentProcessAsync();
    }
}