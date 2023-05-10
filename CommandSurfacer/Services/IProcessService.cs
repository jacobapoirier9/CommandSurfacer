namespace CommandSurfacer.Services;

public interface IProcessService
{
    public string Run(string exeFileName, string arguments);
}
