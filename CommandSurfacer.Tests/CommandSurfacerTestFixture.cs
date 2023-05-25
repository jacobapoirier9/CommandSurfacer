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
                // TODO: Currently, this happens internally by looping through the registered services and adding anything with a SurfaceAttribute specified.
                // TODO: Eventually, this should be abstracted to a separate helper method, but for now we want to manually use this, to prevent reliance on Client.
                var commandSurfaces = new List<CommandSurface>()
                {
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceOne),
                        TypeAttribute = new SurfaceAttribute("sample-one"),
                        Method = typeof(SampleServiceOne).GetMethod(nameof(SampleServiceOne.MethodOne)),
                        MethodAttribute= new SurfaceAttribute("one")
                    },
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceOne),
                        TypeAttribute = new SurfaceAttribute("sample-one"),
                        Method = typeof(SampleServiceOne).GetMethod(nameof(SampleServiceOne.MethodTwo)),
                        MethodAttribute= new SurfaceAttribute("two")
                    },
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceOne),
                        TypeAttribute = new SurfaceAttribute("sample-one"),
                        Method = typeof(SampleServiceOne).GetMethod(nameof(SampleServiceOne.MethodThree)),
                        MethodAttribute= new SurfaceAttribute("three")
                    },

                    new CommandSurface
                    {
                        Type = typeof(SampleServiceTwo),
                        TypeAttribute = new SurfaceAttribute("sample-two"),
                        Method = typeof(SampleServiceTwo).GetMethod(nameof(SampleServiceTwo.MethodThree)),
                        MethodAttribute= new SurfaceAttribute("three")
                    },
                    new CommandSurface
                    {
                        Type = typeof(SampleServiceTwo),
                        TypeAttribute = new SurfaceAttribute("sample-two"),
                        Method = typeof(SampleServiceTwo).GetMethod(nameof(SampleServiceTwo.MethodFour)),
                        MethodAttribute= new SurfaceAttribute("four")
                    },

                    new CommandSurface
                    {
                        Type = typeof(SampleServiceThree),
                        TypeAttribute = new SurfaceAttribute("sample-three"),
                        Method = typeof(SampleServiceThree).GetMethod(nameof(SampleServiceThree.MethodFive)),
                        MethodAttribute= new SurfaceAttribute("five")
                    }
                };

                services.AddSingleton(commandSurfaces);

                services.AddSingleton<SampleServiceOne>();
                services.AddSingleton<SampleServiceTwo>();
                services.AddSingleton<SampleServiceThree>();

                services.AddSingleton<IStringConverter, StringConverter>();
                services.AddSingleton<IArgsParser, ArgsParser>();
                services.AddSingleton<ICommandRunner, CommandRunner>();
            })
            .Build();
    }

    public void Dispose() => _appHost?.Dispose();
}

[Surface("sample-one")]
public class SampleServiceOne
{
    [Surface("one")]
    public void MethodOne() { }

    [Surface("two")]
    public void MethodTwo() { }

    [Surface("three")]
    public void MethodThree() { }
}

[Surface("sample-two")]
public class SampleServiceTwo
{
    [Surface("three")]
    public void MethodThree() { }

    [Surface("four")]
    public void MethodFour() { }
}

[Surface("sample-five")]
public class SampleServiceThree
{
    [Surface("five")]
    public void MethodFive() { }
}