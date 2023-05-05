namespace CommandSurfacer;

public interface IInteractiveConsole
{
    public void EnterShell();

    public Task EnterShellAsync();
}
