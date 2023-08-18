using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text.RegularExpressions;
using Group = CommandSurfacer.GroupAttribute;

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
                // TODO: Currently, this happens internally by looping through the registered services and adding anything with a SurfaceAttribute specified.
                // TODO: Eventually, this should be abstracted to a separate helper method, but for now we want to manually use this, to prevent reliance on Client.
                var commandSurfaces = new List<CommandSurface>()
                {
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceOne),
                        Group = new Group("sample-one"),
                        Method = typeof(SampleServiceOne).GetMethod(nameof(SampleServiceOne.MethodOne)),
                        Surface= new SurfaceAttribute("one")
                    },
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceOne),
                        Group = new Group("sample-one"),
                        Method = typeof(SampleServiceOne).GetMethod(nameof(SampleServiceOne.MethodTwo)),
                        Surface= new SurfaceAttribute("two")
                    },
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceOne),
                        Group = new Group("sample-one"),
                        Method = typeof(SampleServiceOne).GetMethod(nameof(SampleServiceOne.MethodThree)),
                        Surface= new SurfaceAttribute("three")
                    },

                    new CommandSurface
                    {
                        Type = typeof(SampleServiceTwo),
                        Group = new Group("sample-two"),
                        Method = typeof(SampleServiceTwo).GetMethod(nameof(SampleServiceTwo.MethodThree)),
                        Surface= new SurfaceAttribute("three")
                    },
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceTwo),
                        Group = new Group("sample-two"),
                        Method = typeof(SampleServiceTwo).GetMethod(nameof(SampleServiceTwo.MethodFour)),
                        Surface= new SurfaceAttribute("four")
                    },

                    new CommandSurface
                    {
                        Type = typeof(SampleServiceThree),
                        Group = new Group("sample-three"),
                        Method = typeof(SampleServiceThree).GetMethod(nameof(SampleServiceThree.MethodFive)),
                        Surface= new SurfaceAttribute("five")
                    }
                };

                services.AddSingleton(commandSurfaces);

                services.AddSingleton<SampleServiceOne>();
                services.AddSingleton<SampleServiceTwo>();
                services.AddSingleton<SampleServiceThree>();

                services.AddSingleton<IStringConverter, StringConverter>();
                services.AddSingleton<IStringEnumerableConverter, StringEnumerableConverter>();
                services.AddSingleton<IArgsParser, ArgsParser>();
                services.AddSingleton<ICommandRunner, CommandRunner>();

                services.AddSingleton<InjectedService>();
            })
            .Build();
    }

    public void Dispose() => _appHost?.Dispose();
}

[CommandSurfacer.Group("sample-one")]
public class SampleServiceOne
{
    [Surface("one")]
    public void MethodOne() { }

    [Surface("two")]
    public void MethodTwo() { }

    [Surface("three")]
    public void MethodThree() { }
}

[CommandSurfacer.Group("sample-two")]
public class SampleServiceTwo
{
    [Surface("three")]
    public void MethodThree() { }

    [Surface("four")]
    public void MethodFour() { }
}

[CommandSurfacer.Group("sample-five")]
public class SampleServiceThree
{
    [Surface("five")]
    public void MethodFive() { }
}

public class InjectedService
{
    public const string Success = nameof(Success);

    public string GetSuccess() => Success;
}

public class NotInjectedService
{
    public const string Success = nameof(Success);

    public string GetSuccess() => Success;
}