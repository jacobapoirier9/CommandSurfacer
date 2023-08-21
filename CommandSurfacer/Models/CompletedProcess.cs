namespace CommandSurfacer.Models;

public class CompletedProcess
{
    public int ExitCode { get; set; }

    public string StandardOutputString { get; set; }

    public string StandardErrorString { get; set; }
}
