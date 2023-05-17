namespace CommandSurfacer.Services;

public interface IProcessService
{
    public ProcessResult Run(string exeFileName, string arguments);
}
