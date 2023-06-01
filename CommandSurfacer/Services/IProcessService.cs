namespace CommandSurfacer.Services;

public interface IProcessService
{
    public RunProcessResponse RunProcess(string exeFileName, string arguments);
}
