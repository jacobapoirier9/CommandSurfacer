namespace CommandSurfacer.Services;

public class InteractiveConsoleOptions
{
    public string Banner { get; set; }

    public string Prompt { get; set; }

    public Func<string> PromptFunc { get; set; }

    public Action<Exception> OnError { get; set; }
}
