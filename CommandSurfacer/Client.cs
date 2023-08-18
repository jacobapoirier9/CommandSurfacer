using CommandSurfacer.Models;
using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace CommandSurfacer;

public class Client
{
    private readonly List<CommandSurface> _commandSurfaces = new List<CommandSurface>();
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    private IServiceProvider _serviceProvider;

    private bool _internalServicesRegistered = false;

    private Client() { }
    public static Client Create()
    {
        var client = new Client();
        return client;
    }

    public Client AddServices(Action<IServiceCollection> addServices)
    {
        addServices(_serviceCollection);
        return this;
    }

    public Client AddInteractiveConsole(Action<InteractiveConsoleOptions> configure = null)
    {
        _serviceCollection.TryAddSingleton<IResponseProvider, ConsoleResponseProvider>();
        _serviceCollection.TryAddSingleton<IInteractiveConsole, InteractiveConsole>();

        var options = new InteractiveConsoleOptions
        {
            Banner = null,
            Prompt = " >> ",
            PromptFunc = null,
            OnError = null,
            OnErrorCommand = "help"
        };

        if (configure is not null)
            configure(options);

        _serviceCollection.TryAddSingleton(options);

        return this;
    }

    public Client AddConsoleHelp()
    {
        _serviceCollection.AddSingleton<ISendHelpMessages, SendConsoleHelpMessages>();
        return this;
    }

    private void AddInternalServices()
    {
        if (_internalServicesRegistered == false)
        {
            _serviceCollection.AddSingleton(this);
            _serviceCollection.AddSingleton(_serviceCollection);
            _serviceCollection.AddSingleton(_commandSurfaces);

            _serviceCollection.AddSingleton<IStringConverter, StringConverter>();
            _serviceCollection.AddSingleton<IStringEnumerableConverter, StringEnumerableConverter>();
            _serviceCollection.AddSingleton<IArgsParser, ArgsParser>();
            _serviceCollection.AddSingleton<ICommandRunner, CommandRunner>();
            _serviceCollection.AddSingleton<IProcessService, ProcessService>();

            _internalServicesRegistered = true;
        }
    }

    private void BuildCommandSurfaces()
    {
        _commandSurfaces.Clear();

        var keys = new List<string>();
        foreach (var service in _serviceCollection)
        {
            var implementationType = service.ImplementationType ?? service.ImplementationInstance.GetType();

            var methods = implementationType.GetMethods()
                .Where(m => m.IsPublic && m.DeclaringType == implementationType && !m.IsSpecialName)
                .ToList();

            var groupAttribute = implementationType.GetCustomAttribute<GroupAttribute>();
            foreach (var method in methods)
            {
                var surfaceAttribute = method.GetCustomAttribute<SurfaceAttribute>();
                if (groupAttribute is not null || surfaceAttribute is not null)
                {
                    var key = string.Join(' ', groupAttribute?.Name, surfaceAttribute.Name).Trim(' ');
                    if (keys.Contains(key))
                        throw new InvalidOperationException($"Surface {key} has already been added");

                    keys.Add(key);

                    _commandSurfaces.Add(new CommandSurface
                    {
                        Type = service.ServiceType,
                        Group = groupAttribute,
                        Method = method,
                        Surface = surfaceAttribute
                    });
                }
            }
        }
    }

    public Client Run(string[] args, params object[] additionalParameters) => Run<Client>(string.Join(' ', args), additionalParameters);
    public T Run<T>(string[] args, params object[] additionalParameters) => Run<T>(string.Join(' ', args), additionalParameters);
    public Client Run(string input, params object[] additionalParameters) => Run<Client>(input, additionalParameters);
    public T Run<T>(string input, params object[] additionalParameters) => RunFinalAsync<T>(input, additionalParameters).GetAwaiter().GetResult();

    public async Task<object> RunAsync(string[] args, params object[] additionalParameters) => await RunAsync<object>(string.Join(' ', args), additionalParameters);
    public async Task<T> RunAsync<T>(string[] args, params object[] additionalParameters) => await RunAsync<T>(string.Join(' ', args), additionalParameters);
    public async Task<object> RunAsync(string input, params object[] additionalParameters) => await RunAsync<object>(input, additionalParameters);
    public async Task<T> RunAsync<T>(string input, params object[] additionalParameters) => await RunFinalAsync<T>(input, additionalParameters);

    private async Task<T> RunFinalAsync<T>(string input, params object[] additionalParameters)
    {
        AddInternalServices();
        BuildCommandSurfaces();

        _serviceProvider = _serviceCollection.BuildServiceProvider();

        if (string.IsNullOrEmpty(input))
        {
            var interactiveConsole = _serviceProvider.GetService<IInteractiveConsole>();
            if (interactiveConsole is not null)
            {
                await interactiveConsole.BeginInteractiveModeAsync();
                return await Task.FromResult<T>(default(T));
            }
        }

        var commandRunner = _serviceProvider.GetRequiredService<ICommandRunner>();
        var result = await commandRunner.RunAsync<T>(input, additionalParameters);

        return result;
    }
}