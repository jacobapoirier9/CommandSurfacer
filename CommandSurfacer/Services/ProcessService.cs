using CommandSurfacer.Models;
using System.Diagnostics;
using System.Text;

namespace CommandSurfacer.Services;

public class ProcessService : IProcessService
{
    public RunProcessResponse RunProcess(string exeFileName, string arguments)
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

        process.WaitForExit();

        var standardOutput = outputBuilder.ToString();
        var standardError = errorBuilder.ToString();

        var result = new RunProcessResponse
        {
            ExitCode = process.ExitCode,
            StandardOutput = standardOutput.Any() ? standardOutput : null,
            StandardError = standardOutput.Any() ? standardError : null
        };

        if (result.ExitCode > 0)
            throw new InvalidProgramException($"Process finished with exit code {result.ExitCode}: {result.StandardError + result.StandardOutput}");

        return result;
    }

    public async Task RunProcessAsync(string exeFileName, string arguments)
    {
        await new Task(() => RunProcess(exeFileName, arguments));
    }
}
