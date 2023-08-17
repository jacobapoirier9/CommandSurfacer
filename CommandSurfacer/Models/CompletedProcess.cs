using System.Diagnostics;

namespace CommandSurfacer.Models;

public class CompletedProcess : Process
{
    public string StandardOutputString { get; set; }

    public string StandardErrorString { get; set; }
}
