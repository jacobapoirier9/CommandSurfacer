using CommandSurfacer.Models;
using System.Diagnostics;
using System.Text;

namespace CommandSurfacer.Services;

public class ProcessService : IProcessService
{
    private readonly IStringConverter _stringConverter;
    public ProcessService(IStringConverter stringConverter)
    {
        _stringConverter = stringConverter;
    }

    public async Task<Process> GetParentProcessAsync()
    {
        var process = Process.GetCurrentProcess();
        var encodedCommand = Utils.PowerShellEncodeCommand($"Get-CimInstance Win32_Process -Filter \"ProcessId = '{process.Id}'\" | select ParentProcessId -ExpandProperty ParentProcessId");

        var completed = await RunAsync("powershell.exe", encodedCommand);

        var parentProcessId = _stringConverter.Convert<int>(completed.StandardOutputString);

        var parentProcess = Process.GetProcessById(parentProcessId);
        return parentProcess;
    }

    public async Task<CompletedProcess> RunAsync(string exeFileName, string arguments)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            RedirectStandardInput = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,

            FileName = exeFileName,
            UseShellExecute = false,

            CreateNoWindow = true,
            Arguments = arguments,
        });

        var outputBuilder = new StringBuilder();
        process.OutputDataReceived += (obj, data) => outputBuilder.AppendLine(data.Data);

        var errorBuilder = new StringBuilder();
        process.ErrorDataReceived += (obj, data) => errorBuilder.AppendLine(data.Data);

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();

        var completedProcess = new CompletedProcess
        {
            ExitCode = process.ExitCode,
            StandardOutputString = outputBuilder.ToString(),
            StandardErrorString = errorBuilder.ToString(),
        };

        if (process.ExitCode > 0)
            throw new InvalidProgramException($"Process finished with exit code {completedProcess.ExitCode}: {completedProcess.StandardErrorString + completedProcess.StandardOutputString}");

        return completedProcess;
    }
}