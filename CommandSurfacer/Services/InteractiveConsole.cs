using CommandSurfacer.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandSurfacer.Services;

public class InteractiveConsole : IInteractiveConsole
{
    public const string EnterInteractiveConsoleCommand = "enter-interactive";

    private readonly ICommandRunner _commandRunner;
    private readonly IGetInput _getInput;
    private readonly InteractiveConsoleOptions _options;

    private bool _continue;

    public InteractiveConsole(ICommandRunner commandRunner, IGetInput getInput, IServiceProvider serviceProvider)
    {
        _commandRunner = commandRunner;
        _getInput = getInput;
        _options = serviceProvider.GetService<InteractiveConsoleOptions>();

        _continue = true;
    }

    [Surface(EnterInteractiveConsoleCommand, ExcludeFromHelp = true)]
    public void BeginInteractiveMode() => BeginInteractiveModeAsync().GetAwaiter().GetResult();
    public async Task BeginInteractiveModeAsync()
    {
        if (_options?.Banner is not null)
            Console.WriteLine(_options.Banner);

        while (_continue)
        {
            var prompt = _options.Prompt ?? _options.PromptFunc?.Invoke();
            var line = _getInput.GetInput(prompt);

            try
            {
                await _commandRunner.RunAsync(line);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (_options.OnError is not null)
                    _options.OnError(ex);

                if (_options.OnErrorCommand is not null)
                    await _commandRunner.RunAsync("/?");

            }
        }
    }

    [Surface("exit")]
    public void EndInteractiveMode()
    {
        _continue = false;
    }
}
