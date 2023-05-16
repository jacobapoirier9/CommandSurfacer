using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommandSurfacer.Tests;

public class ArgsParserTests : BaseTests
{
    private readonly IArgsParser _argsParser;

    public ArgsParserTests(CommandSurfacerTestFixture fixture)
    {
        _argsParser = fixture.AppHost.Services.GetRequiredService<IArgsParser>();
    }
}
