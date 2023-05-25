using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography.X509Certificates;

namespace CommandSurfacer.Tests;

public class ArgsParserTests : BaseTests
{
    private readonly IArgsParser _argsParser;

    public ArgsParserTests(CommandSurfacerTestFixture fixture)
    {
        _argsParser = fixture.AppHost.Services.GetRequiredService<IArgsParser>();
    }

    #region Parse Command Surface
    [Fact]
    public void ParseCommandSurface_ResolveSingleSurface_SpecifiedByActionWithOnlyOneActionMatch()
    {
        var input = "one";
        var surface = _argsParser.ParseCommandSurface(ref input);

        Assert.Empty(input);

        Assert.Equal("sample-one", surface.TypeAttribute.Name);
        Assert.Equal(nameof(SampleServiceOne), surface.Type.Name);

        Assert.Equal("one", surface.MethodAttribute.Name);
        Assert.Equal(nameof(SampleServiceOne.MethodOne), surface.Method.Name);
    }

    [Fact]
    public void ParseCommandSurface_ThrowsException_SpecifiedByActionWithDuplicateActions()
    {
        var input = "three";

        Assert.ThrowsAny<Exception>(() => _argsParser.ParseCommandSurface(ref input));
    }

    [Fact]
    public void ParseCommandSurface_ResolveSingleSurface_SpecifiedByControllerAndActionWithDuplicateActions()
    {
        var input = "sample-two three";

        var surface = _argsParser.ParseCommandSurface(ref input);

        Assert.Empty(input);

        Assert.Equal("sample-two", surface.TypeAttribute.Name);
        Assert.Equal(nameof(SampleServiceTwo), surface.Type.Name);

        Assert.Equal("three", surface.MethodAttribute.Name);
        Assert.Equal(nameof(SampleServiceTwo.MethodThree), surface.Method.Name);
    }

    [Fact]
    public void ParseCommandSurface_ResolveSingleSurface_SpecifiedByControllerWithTwoActionMatches()
    {
        var input = "sample-three";
        var surface = _argsParser.ParseCommandSurface(ref input);

        Assert.Empty(input);

        Assert.Equal("sample-three", surface.TypeAttribute.Name);
        Assert.Equal(nameof(SampleServiceThree), surface.Type.Name);

        Assert.Equal("five", surface.MethodAttribute.Name);
        Assert.Equal(nameof(SampleServiceThree.MethodFive), surface.Method.Name);
    }
    #endregion

    #region Parse Presence Value
    [Theory]
    [InlineData("--test-name")]
    [InlineData("--test-name 1")]
    [InlineData("--test-name y")]
    [InlineData("--test-name yes")]
    [InlineData("--test-name true")]
    [InlineData("--test-name Y")]
    [InlineData("--test-name YES")]
    [InlineData("--test-name TRUE")]
    public void ParsePresenceValue_Bool_TrueByValue_Beginning(string inputValue)
    {
        var input = $"{inputValue}   --t1 notused --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool));

        Assert.NotNull(output);
        Assert.True(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name")]
    [InlineData("--test-name 1")]
    [InlineData("--test-name y")]
    [InlineData("--test-name yes")]
    [InlineData("--test-name true")]
    [InlineData("--test-name Y")]
    [InlineData("--test-name YES")]
    [InlineData("--test-name TRUE")]
    public void ParsePresenceValue_Bool_TrueByValue_Middle(string inputValue)
    {
        var input = $"--t1 notused   {inputValue}  --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool));

        Assert.NotNull(output);
        Assert.True(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name")]
    [InlineData("--test-name 1")]
    [InlineData("--test-name y")]
    [InlineData("--test-name yes")]
    [InlineData("--test-name true")]
    [InlineData("--test-name Y")]
    [InlineData("--test-name YES")]
    [InlineData("--test-name TRUE")]
    public void ParsePresenceValue_Bool_TrueByValue_End(string inputValue)
    {
        var input = $"--t1 notused --t2 notused      {inputValue}";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool));

        Assert.NotNull(output);
        Assert.True(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name")]
    [InlineData("--test-name 1")]
    [InlineData("--test-name y")]
    [InlineData("--test-name yes")]
    [InlineData("--test-name true")]
    [InlineData("--test-name Y")]
    [InlineData("--test-name YES")]
    [InlineData("--test-name TRUE")]
    public void ParsePresenceValue_NullableBool_TrueByValue_Beginning(string inputValue)
    {
        var input = $"{inputValue}   --t1 notused --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool?));

        Assert.NotNull(output);
        Assert.True(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name")]
    [InlineData("--test-name 1")]
    [InlineData("--test-name y")]
    [InlineData("--test-name yes")]
    [InlineData("--test-name true")]
    [InlineData("--test-name Y")]
    [InlineData("--test-name YES")]
    [InlineData("--test-name TRUE")]
    public void ParsePresenceValue_NullableBool_TrueByValue_Middle(string inputValue)
    {
        var input = $"--t1 notused   {inputValue}  --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool?));

        Assert.NotNull(output);
        Assert.True(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name")]
    [InlineData("--test-name 1")]
    [InlineData("--test-name y")]
    [InlineData("--test-name yes")]
    [InlineData("--test-name true")]
    [InlineData("--test-name Y")]
    [InlineData("--test-name YES")]
    [InlineData("--test-name TRUE")]
    public void ParsePresenceValue_NullableBool_TrueByValue_End(string inputValue)
    {
        var input = $"--t1 notused --t2 notused      {inputValue}";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool?));

        Assert.NotNull(output);
        Assert.True(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name 0")]
    [InlineData("--test-name n")]
    [InlineData("--test-name no")]
    [InlineData("--test-name false")]
    [InlineData("--test-name N")]
    [InlineData("--test-name NO")]
    [InlineData("--test-name FALSE")]
    public void ParsePresenceValue_Bool_FalseByValue_Beginning(string inputValue)
    {
        var input = $"{inputValue}   --t1 notused --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool));

        Assert.NotNull(output);
        Assert.False(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name 0")]
    [InlineData("--test-name n")]
    [InlineData("--test-name no")]
    [InlineData("--test-name false")]
    [InlineData("--test-name N")]
    [InlineData("--test-name NO")]
    [InlineData("--test-name FALSE")]
    public void ParsePresenceValue_Bool_FalseByValue_Middle(string inputValue)
    {
        var input = $"--t1 notused   {inputValue}  --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool));

        Assert.NotNull(output);
        Assert.False(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name 0")]
    [InlineData("--test-name n")]
    [InlineData("--test-name no")]
    [InlineData("--test-name false")]
    [InlineData("--test-name N")]
    [InlineData("--test-name NO")]
    [InlineData("--test-name FALSE")]
    public void ParsePresenceValue_Bool_FalseByValue_End(string inputValue)
    {
        var input = $"--t1 notused --t2 notused      {inputValue}";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool));

        Assert.NotNull(output);
        Assert.False(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name 0")]
    [InlineData("--test-name n")]
    [InlineData("--test-name no")]
    [InlineData("--test-name false")]
    [InlineData("--test-name N")]
    [InlineData("--test-name NO")]
    [InlineData("--test-name FALSE")]
    public void ParsePresenceValue_NullableBool_FalseByValue_Beginning(string inputValue)
    {
        var input = $"{inputValue}   --t1 notused --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool?));

        Assert.NotNull(output);
        Assert.False(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name 0")]
    [InlineData("--test-name n")]
    [InlineData("--test-name no")]
    [InlineData("--test-name false")]
    [InlineData("--test-name N")]
    [InlineData("--test-name NO")]
    [InlineData("--test-name FALSE")]
    public void ParsePresenceValue_NullableBool_FalseByValue_Middle(string inputValue)
    {
        var input = $"--t1 notused   {inputValue}  --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool?));

        Assert.NotNull(output);
        Assert.False(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Theory]
    [InlineData("--test-name 0")]
    [InlineData("--test-name n")]
    [InlineData("--test-name no")]
    [InlineData("--test-name false")]
    [InlineData("--test-name N")]
    [InlineData("--test-name NO")]
    [InlineData("--test-name FALSE")]
    public void ParsePresenceValue_NullableBool_FalseByValue_End(string inputValue)
    {
        var input = $"--t1 notused --t2 notused      {inputValue}";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool?));

        Assert.NotNull(output);
        Assert.False(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Fact]
    public void ParsePresenceValue_Bool_FalseByAbesnce()
    {
        var input = "--t1 notused --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool));

        Assert.NotNull(output);
        Assert.False(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }

    [Fact]
    public void ParsePresenceValue_NullableBool_NullByAbsense()
    {
        var input = "--t1 notused --t2 notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParsePresenceValue(ref input, surface, typeof(bool?));

        Assert.Null(output);
        Assert.Equal("--t1 notused --t2 notused", input);
    }
    #endregion

    #region Parse String Value

    [Theory]
    [InlineData("--test-name value")]
    [InlineData("--test-name 'value'")]
    [InlineData("--test-name \"value\"")]
    [InlineData("--test-name D:\\Path\\With\\No\\Spaces\\File.txt")]
    [InlineData("--test-name 'D:\\Path\\With\\No\\Spaces\\File.txt'")]
    [InlineData("--test-name \"D:\\Path\\With\\No\\Spaces\\File.txt\"")]
    [InlineData("--test-name 'D:\\Path\\With Spaces\\File.txt'")]
    [InlineData("--test-name \"D:\\Path\\With Spaces\\File.txt\"")]
    [InlineData("--test-name 'Single Outer, \"Double inner\"'")]
    [InlineData("--test-name \"Double Outer, 'Single inner'\"")]
    public void ParseStringValue_ReturnsValidValue(string inputValue)
    {
        var input = $"--notused junk   {inputValue}   --notused";
        var surface = new SurfaceAttribute("test-name");

        var expected = inputValue.Replace("--test-name", string.Empty).Trim(' ');
        if (
            (expected.StartsWith('"') && expected.EndsWith('"')) ||
            (expected.StartsWith("'") && expected.EndsWith("'"))
        )
            expected = expected.Substring(1, expected.Length - 2);


        var output = _argsParser.ParseStringValue(ref input, surface);

        Assert.NotNull(output);
        Assert.Equal(expected, output);
        Assert.Equal("--notused junk --notused", input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("--test-name")]
    [InlineData("--test-name ")]
    [InlineData("--test-name  ")]
    public void ParseStringValue_ReturnsEmptyString(string inputValue)
    {
        var input = $"--notused junk   {inputValue}   --notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParseStringValue(ref input, surface);
        Assert.Null(output);
    }

    [Fact]
    public void ParseStringValue_ParsesUnquotedStringPartially()
    {
        var input = $"--notused junk   --test-name D:\\Path\\With Spaces\\File.txt   --notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParseStringValue(ref input, surface);
        Assert.Equal("D:\\Path\\With", output);
        Assert.Equal("--notused junk Spaces\\File.txt   --notused", input);
    }
    #endregion
}
