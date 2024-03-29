﻿using CommandSurfacer.Models;
using System.Diagnostics;

namespace CommandSurfacer.Services;

public interface IProcessService
{
    public Process GetParentProcess(int? processId = null) => GetParentProcessAsync(processId).GetAwaiter().GetResult();
    public Task<Process> GetParentProcessAsync(int? processId = null);


    public CompletedProcess Run(string exeFileName, string arguments) => RunAsync(exeFileName, arguments).GetAwaiter().GetResult();
    public Task<CompletedProcess> RunAsync(string exeFileName, string arguments);
}
