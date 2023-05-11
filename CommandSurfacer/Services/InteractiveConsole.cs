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
    public void BeginInteractiveMode() => BeginInteractiveModeAsync().GetAwaiter().GetResult();
    public async Task BeginInteractiveModeAsync()
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

            try
            {
                await _commandRunner.RunAsync(line);
            }
            catch (Exception ex)
            {
                if (_options.OnError is not null)
                    _options.OnError(this, ex);
                else
                    Console.WriteLine(ex);
            }
        }
    }

    [Surface("exit")]
    public void EndInteractiveMode()
    {
        _continue = false;
    }
}
