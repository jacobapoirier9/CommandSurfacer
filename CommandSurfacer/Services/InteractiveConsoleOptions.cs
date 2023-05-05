namespace CommandSurfacer;

public class InteractiveConsoleOptions
{
    public string Banner { get; set; }

    public string Prompt { get; set; }

    public Func<string> PromptFunc { get; set; }
}
