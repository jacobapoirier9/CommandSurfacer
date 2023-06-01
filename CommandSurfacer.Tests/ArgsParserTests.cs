﻿using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;

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
    public void ParseStringValue_ReturnsValidValue_Beginning(string inputValue)
    {
        var input = $"{inputValue}     --notused junk --notused";
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
    public void ParseStringValue_ReturnsValidValue_Middle(string inputValue)
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
    public void ParseStringValue_ReturnsValidValue_End(string inputValue)
    {
        var input = $"--notused junk --notused   {inputValue}";
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
    public void ParseStringValue_ReturnsEmptyString_Beginning(string inputValue)
    {
        var input = $"{inputValue}     --notused junk --notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParseStringValue(ref input, surface);
        Assert.Null(output);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("--test-name")]
    [InlineData("--test-name ")]
    [InlineData("--test-name  ")]
    public void ParseStringValue_ReturnsEmptyString_Middle(string inputValue)
    {
        var input = $"--notused junk   {inputValue}   --notused";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParseStringValue(ref input, surface);
        Assert.Null(output);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("--test-name")]
    [InlineData("--test-name ")]
    [InlineData("--test-name  ")]
    public void ParseStringValue_ReturnsEmptyString_End(string inputValue)
    {
        var input = $"--notused junk    --notused   {inputValue}";
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

    #region Parse Typed Value
    [Fact] public void ParseTypedValue_Byte() => ParseTypedValue_CorrectTypeMapping<byte>(1);
    [Fact] public void ParseTypedValue_Short() => ParseTypedValue_CorrectTypeMapping<short>(1);
    [Fact] public void ParseTypedValue_Int() => ParseTypedValue_CorrectTypeMapping<int>(1);
    [Fact] public void ParseTypedValue_Long() => ParseTypedValue_CorrectTypeMapping<long>(1);
    [Fact] public void ParseTypedValue_Double() => ParseTypedValue_CorrectTypeMapping<double>(1);
    [Fact] public void ParseTypedValue_Float() => ParseTypedValue_CorrectTypeMapping<float>(1);
    [Fact] public void ParseTypedValue_Decimal() => ParseTypedValue_CorrectTypeMapping<decimal>(1);

    [Fact] public void ParseTypedValue_NullableByte() => ParseTypedValue_CorrectTypeMapping<byte?>(null);
    [Fact] public void ParseTypedValue_NullableShort() => ParseTypedValue_CorrectTypeMapping<short?>(null);
    [Fact] public void ParseTypedValue_NullableInt() => ParseTypedValue_CorrectTypeMapping<int?>(null);
    [Fact] public void ParseTypedValue_NullableLong() => ParseTypedValue_CorrectTypeMapping<long?>(null);
    [Fact] public void ParseTypedValue_NullableDouble() => ParseTypedValue_CorrectTypeMapping<double?>(null);
    [Fact] public void ParseTypedValue_NullableFloat() => ParseTypedValue_CorrectTypeMapping<float?>(null);
    [Fact] public void ParseTypedValue_NullableDecimal() => ParseTypedValue_CorrectTypeMapping<decimal?>(null);

    private void ParseTypedValue_CorrectTypeMapping<T>(T inputAndExpectedValue)
    {
        var targetType = typeof(T);

        var input = $"--test-name {inputAndExpectedValue}";
        var surface = new SurfaceAttribute("test-name");

        var output = _argsParser.ParseTypedValue(ref input, surface, targetType);

        if (output is not null) 
            Assert.Equal(targetType, output.GetType());
        Assert.Equal(output, inputAndExpectedValue);
    }

    [Fact]
    public void ParseTypedValue_CreatesStrongType()
    {
        var input = string.Join(' ', new string[]
        {
            "--string value",
            "--byte 1 --short 1 --int 1 --long 1 --double 1 --float 1 --decimal 1 --bool yes",
            "--file .\\Sandbox\\TestFile.txt --directory .\\Sandbox"
        });

        var parsed = (ParseTypedValue_StrongType)_argsParser.ParseTypedValue(ref input, null, typeof(ParseTypedValue_StrongType));

        Assert.Equal("value", parsed.String);
        Assert.Equal(1, parsed.Byte);
        Assert.Equal(1, parsed.Short);
        Assert.Equal(1, parsed.Int);
        Assert.Equal(1, parsed.Long);
        Assert.Equal(1, parsed.Double);
        Assert.Equal(1, parsed.Float);
        Assert.Equal(1, parsed.Decimal);
        Assert.True(parsed.Bool);

        Assert.NotNull(parsed.File);
        Assert.True(parsed.File.Exists);
        Assert.Equal("TestFile.txt", parsed.File.Name);
        
        Assert.NotNull(parsed.Directory);
        Assert.True(parsed.Directory.Exists);
        Assert.Equal("Sandbox", parsed.Directory.Name);
    }

    [Fact]
    public void ParseTypedValue_CreatesStrongTypeWithNestedType()
    {
        var input = "--name-one ONE --name-two TWO";

        var parsed = (ParseTypedValue_Parent)_argsParser.ParseTypedValue(ref input, null, typeof(ParseTypedValue_Parent));

        Assert.NotNull(parsed);
        Assert.Equal("ONE", parsed.NameOne);
        Assert.NotNull(parsed.InjectedService);
        Assert.Equal(InjectedService.Success, parsed.InjectedService.GetSuccess());

        Assert.NotNull(parsed.Child);
        Assert.Equal("TWO", parsed.Child.NameTwo);
        Assert.Equal(InjectedService.Success, parsed.Child.InjectedService.GetSuccess());
    }

    private class ParseTypedValue_StrongType
    {
        [Surface(nameof(String))]
        public string String { get; set; }

        [Surface(nameof(Byte))]
        public byte Byte { get; set; }

        [Surface(nameof(Short))]
        public short Short { get; set; }

        [Surface(nameof(Int))]
        public int Int { get; set; }

        [Surface(nameof(Long))]
        public long Long { get; set; }

        [Surface(nameof(Double))]
        public double Double { get; set; }

        [Surface(nameof(Float))]
        public float Float { get; set; }

        [Surface(nameof(Decimal))]
        public decimal Decimal { get; set; }

        [Surface(nameof(Bool))]
        public bool Bool { get; set; }

        [Surface(nameof(File))]
        public FileInfo File { get; set; }

        [Surface(nameof(Directory))]
        public DirectoryInfo Directory { get; set; }
    }

    private class ParseTypedValue_Parent
    {
        [Surface("name-one")]
        public string NameOne { get; set; }

        public ParseTypedValue_Child Child { get; set; }

        public InjectedService InjectedService { get; set; }
    }

    private class ParseTypedValue_Child
    {
        [Surface("name-two")]
        public string NameTwo { get; set; }

        public InjectedService InjectedService { get; set; }
    }
    #endregion

    #region Get Special Value
    [Fact]
    public void GetSpecialValue_TextReader()
    {
        var standardInput = _argsParser.GetSpecialValue(null, typeof(TextReader));
        Assert.NotNull(standardInput);

        Assert.Equal(Console.In, standardInput);
    }

    [Fact]
    public void GetSpecialValue_TextWriter()
    {
        var standardOutput = _argsParser.GetSpecialValue(null, typeof(TextWriter));
        Assert.NotNull(standardOutput);

        Assert.Equal(Console.Out, standardOutput);
    }

    [Fact]
    public void GetSpecialValue_ServiceCollection()
    {
        var serviceProvider = _argsParser.GetSpecialValue(null, typeof(IServiceProvider));
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public void GetSpecialValue_Null()
    {
        var result = _argsParser.GetSpecialValue(null, typeof(string));
        Assert.Null(result);
    }
    #endregion
}
