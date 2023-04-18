﻿using Microsoft.Extensions.DependencyInjection;

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
            });

        client.Run(args);


    }
}

[Surface("c")]
public class TestService
{
    [Surface("test")]
    public async Task Test(FileInfo file)
    {
    }
}

