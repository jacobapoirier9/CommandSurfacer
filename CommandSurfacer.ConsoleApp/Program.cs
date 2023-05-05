using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace CommandSurfacer.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    private static async Task MainAsync(string[] args)
    {
        args = new string[] { };

        var client = Client.Create()
            .AddInteractiveConsole(options =>
            {
                options.Banner = "Welcome to Jake's custom CLI";
            })
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
    public async Task Test()
    {
    }
}
