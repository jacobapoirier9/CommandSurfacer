//using CommandSurfacer.Services;
//using Microsoft.Extensions.DependencyInjection;

//namespace CommandSurfacer.Tests;

//public class BetterArgsParserTests : BaseTests
//{
//    private readonly IBetterArgsParser _argsParser;

//    public BetterArgsParserTests(CommandSurfacerTestFixture fixture)
//    {
//        _argsParser = fixture.AppHost.Services.GetRequiredService<IBetterArgsParser>();
//    }

//    [Theory]
//    [InlineData("arg1 arg2 arg3", new string[] { "arg1", "arg2", "arg3" })]
//    [InlineData("'arg1' 'arg2' arg3'", new string[] { "arg1", "arg2", "arg3" })]
//    [InlineData("\"arg1\" \"arg2\" \"arg3\"", new string[] { "arg1", "arg2", "arg3" })]

//    [InlineData("arg 1 arg 2 arg 3", new string[] { "arg", "1", "arg", "2", "arg", "3" })]
//    [InlineData("'arg 1' 'arg 2' 'arg 3'", new string[] { "arg 1", "arg 2", "arg 3" })]
//    [InlineData("\"arg 1\" \"arg 2\" \"arg 3\"", new string[] { "arg 1", "arg 2", "arg 3" })]

//    [InlineData("arg,1, arg,2, arg,3,", new string[] { "arg", "1", "arg", "2", "arg", "3" })]
//    [InlineData("'arg,1', 'arg,2', 'arg,3',", new string[] { "arg,1", "arg,2", "arg,3" })]
//    [InlineData("\"arg,1\", \"arg,2\", \"arg,3\",", new string[] { "arg,1", "arg,2", "arg,3" })]

//    public void ParseArguments_SeparatesNamedArgumentsCorrectly(string input, string[] expectedArray)
//    { 
//        var actual = _argsParser.Parse(input);
//        var expected = new Arguments
//        {
//            List = expectedArray.ToList(),
//            Dictionary = new Dictionary<string, List<string>> { }
//        };

//        AssertList(expected.List, actual.List);
//        Assert.Empty(actual.Dictionary);
//    }


//    [Theory]
//    [InlineData("--arg value")]
//    [InlineData("--arg=value")]
//    [InlineData("--arg:value")]
//    public void ParseArguments_SplitsUsingKeyValueTerminators(string input)
//    {
//        var actual = _argsParser.Parse(input);
//        var expected = new Arguments
//        {
//            List = new List<string>
//            {
//            },
//            Dictionary = new Dictionary<string, List<string>>
//            {
//                { "arg", new List<string> { "value" } }
//            },
//        };

//        AssertList(expected.List, actual.List);
//        AssertDictionary(expected.Dictionary, actual.Dictionary);

//    }


//    private void AssertList<T>(IEnumerable<T> expected, IEnumerable<T> actual) => Assert.Equal(expected, actual);

//    private void AssertDictionary<TKey, TValue>(IDictionary<TKey, TValue> expected, IDictionary<TKey, TValue> actual)
//    {
//        Assert.Equal(expected.Keys.Count, actual.Keys.Count);
//        Assert.Equal(expected.Keys.Count(), (from expectedKey in expected.Keys
//                                             join actualKey in actual.Keys
//                                               on expectedKey equals actualKey
//                                             select actualKey).Count());
//    }
//}
