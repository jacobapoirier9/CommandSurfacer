using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommandSurfacer.ConsoleApp;

internal static class Program
{
    private static void Main(string[] args) => MainAsync(args).GetAwaiter().GetResult();
    private static async Task MainAsync(string[] args)
    {
        args = new string[] { "c  'Anno' --file 'Jake'" };

        var client = Client.Create()
            .AddServices(services =>

            {
                services.AddSingleton<TestService>();
                services.AddSingleton<IResponseProvider>(new MemoryResponseProvider { Responses = new List<object> { "Test" } });
            });

        client.Run(args);


    }
}

[Surface("c")]
public class TestService
{
    [Surface("test")]
    public async Task Test(IResponseProvider responseProvider)
    {
        var first = responseProvider.GetResponse("");
    }
}
