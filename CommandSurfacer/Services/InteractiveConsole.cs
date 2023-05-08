using CommandSurfacer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CommandSurfacer;

public class InteractiveConsole : IInteractiveConsole
{
    public const string EnterInteractiveConsoleCommand = "interactive";

    private readonly ICommandRunner _commandRunner;
    private readonly InteractiveConsoleOptions _options;

    private bool _continue;

    public InteractiveConsole(ICommandRunner commandRunner, IServiceProvider serviceProvider)
    {
        _commandRunner = commandRunner;
        _options = serviceProvider.GetService<InteractiveConsoleOptions>();

        _continue = true;
    }

    [Surface(EnterInteractiveConsoleCommand)]
    public void EnterShell() => EnterShellAsync().GetAwaiter().GetResult();
    public async Task EnterShellAsync()
    {
        if (_options.Banner is not null)
            Console.WriteLine(_options.Banner);

        while (_continue)
        {
            if (_options.Prompt is not null)
                Console.Write(_options.Prompt);
            else if (_options.PromptFunc is not null)
                Console.Write(_options.PromptFunc());

            var line = Console.ReadLine();

            await _commandRunner.RunAsync(line);
        }
    }

    [Surface("exit")]
    public async Task ExitShell()
    {
        _continue = false;

        await Task.CompletedTask;
    }
}
