using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace CommandSurfacer.Tests;

public class CommandSurfacerTestFixture : IDisposable
{
    private static string AssemblyExePath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory;

    private readonly IHost _appHost;
    public IHost AppHost => _appHost;

    public CommandSurfacerTestFixture()
    {
        _appHost = Host.CreateDefaultBuilder()
            .UseContentRoot(AssemblyExePath)
            .ConfigureServices(services =>
            {
                var commandSurfaces = new List<CommandSurface>();

                services.AddSingleton(commandSurfaces);
                services.AddSingleton<IStringConverter, StringConverter>();
                services.AddSingleton<IArgsParser, ArgsParser>();
                services.AddSingleton<ICommandRunner, CommandRunner>();
            })
            .Build();
    }

    public void Dispose() => _appHost?.Dispose();
}
