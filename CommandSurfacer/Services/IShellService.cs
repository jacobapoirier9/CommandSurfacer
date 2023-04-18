namespace CommandSurfacer.Services;

public interface IShellService
{
    public string Run(string exeFileName, string arguments);
}
