using CommandSurfacer.Models;
using System.Diagnostics;
using System.Text;

namespace CommandSurfacer.Services;

public class ProcessService : IProcessService
{
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

        var completedProcess = process as CompletedProcess;
        completedProcess.StandardOutputString = outputBuilder.ToString();
        completedProcess.StandardErrorString = errorBuilder.ToString();

        if (process.ExitCode > 0)
            throw new InvalidProgramException($"Process finished with exit code {completedProcess.ExitCode}: {completedProcess.StandardErrorString + completedProcess.StandardOutputString}");

        return completedProcess;
    }
}