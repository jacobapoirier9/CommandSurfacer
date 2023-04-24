using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CommandSurfacer;

public class Client
{
    protected readonly List<CommandSurface> CommandSurfaces = new List<CommandSurface>();
    protected readonly IServiceCollection ServiceCollection = new ServiceCollection();

    protected IServiceProvider ServiceProvider;

    private bool _internalServicesRegistered = false;

    private Client() { }
    public static Client Create()
    {
        var client = new Client();
        return client;
    }

    public Client AddServices(Action<IServiceCollection> addServices)
    {
        addServices(ServiceCollection);
        return this;
    }

    private void AddInternalServices()
    {
        if (_internalServicesRegistered == false)
        {
            ServiceCollection.AddSingleton(this);
            ServiceCollection.AddSingleton(ServiceCollection);
            ServiceCollection.AddSingleton(CommandSurfaces);

            ServiceCollection.AddSingleton<IStringConverter, StringConverter>();
            ServiceCollection.AddSingleton<IArgsParser, ArgsParser>();
            ServiceCollection.AddSingleton<ICommandRunner, CommandRunner>();
            ServiceCollection.AddSingleton<IShellService, ShellService>();

            _internalServicesRegistered = true;
        }
    }

    private void BuildCommandSurfaces()
    {
        CommandSurfaces.Clear();

        foreach (var service in ServiceCollection)
        {
            var methods = service.ImplementationType.GetMethods()
                .Where(m => m.IsPublic && m.DeclaringType == service.ImplementationType && !m.IsSpecialName)
                .ToList();

            var typeAttribute = service.ImplementationType.GetCustomAttribute<SurfaceAttribute>();
            foreach (var method in methods)
            {
                var methodAttribute = method.GetCustomAttribute<SurfaceAttribute>();
                if (typeAttribute is not null || methodAttribute is not null) // We only want to add a command surface if it has been explicitly defined as a surface.
                {
                    CommandSurfaces.Add(new CommandSurface
                    {
                        Type = service.ServiceType,
                        TypeAttribute = typeAttribute,
                        Method = method,
                        MethodAttribute = methodAttribute
                    });
                }
            }
        }
    }

    private void FinalizeClientBuild()
    {
        BuildCommandSurfaces();
        AddInternalServices();

        ServiceProvider = ServiceCollection.BuildServiceProvider();
    }

    public Client Run(string[] args, params object[] additionalParameters) => Run<Client>(string.Join(' ', args), additionalParameters);
    public T Run<T>(string[] args, params object[] additionalParameters) => Run<T>(string.Join(' ', args), additionalParameters);
    public Client Run(string input, params object[] additionalParameters) => Run<Client>(input, additionalParameters);
    public T Run<T>(string input, params object[] additionalParameters)
    {
        FinalizeClientBuild();

        var commandRunner = ServiceProvider.GetRequiredService<ICommandRunner>();
        var result = commandRunner.Run<T>(input, additionalParameters);

        if (typeof(T) == typeof(Client))
            return (T)(object)this;

        return result;
    }

    public async Task<object> RunAsync(string[] args, params object[] additionalParameters) => await RunAsync<object>(string.Join(' ', args), additionalParameters);
    public async Task<T> RunAsync<T>(string[] args, params object[] additionalParameters) => await RunAsync<T>(string.Join(' ', args), additionalParameters);
    public async Task<object> RunAsync(string input, params object[] additionalParameters) => await RunAsync<object>(input, additionalParameters);
    public async Task<T> RunAsync<T>(string input, params object[] additionalParameters)
    {
        FinalizeClientBuild();

        var commandRunner = ServiceProvider.GetRequiredService<ICommandRunner>();
        var result = await commandRunner.RunAsync<T>(input, additionalParameters);

        return result;
    }
}
