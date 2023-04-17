using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommandSurfacer.Tests;

public class CommandRunnerTests
{
    private readonly Client _client;

    public CommandRunnerTests()
    {
        _client = Client.Create()
            .AddServices(services =>
            {
                services.AddSingleton<CommandRunnerTestsService>();
            });
    }

    [Fact]
    public void ReturnsCorrectResult_CallSyncViaSync()
    {
        var result = _client.Run<Person>("get-person");

        Assert.NotNull(result);
        Assert.Equal("Jake", result.Name);
    }

    [Fact]
    public void ReturnsCorrectResult_CallAsyncViaSync()
    {
        var result = _client.Run<Person>("get-person-async");

        Assert.NotNull(result);
        Assert.Equal("Jake", result.Name);
    }


    [Fact]
    public async Task ReturnsCorrectResult_CallSyncViaAsync()
    {
        var result = await _client.RunAsync<Person>("get-person");

        Assert.NotNull(result);
        Assert.Equal("Jake", result.Name);
    }

    [Fact]
    public async Task ReturnsCorrectResult_CallAsyncViaAsync()
    {
        var result = await _client.RunAsync<Person>("get-person-async");

        Assert.NotNull(result);
        Assert.Equal("Jake", result.Name);
    }






    public class CommandRunnerTestsService
    {
        private Person CreatePerson(string name)
        {
            return new Person { Name = name };
        }


        [Surface("get-person")]
        public Person GetPerson() => CreatePerson("Jake");

        [Surface("get-person-async")]
        public async Task<Person> GetPersonAsync() => await Task.FromResult(CreatePerson("Jake"));
    }
}

public class Person
{
    public string Name { get; set; }
}