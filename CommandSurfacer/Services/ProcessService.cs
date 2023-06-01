using System.Diagnostics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CommandSurfacer.Services;

public class ProcessService : IProcessService
{
    public ProcessResult Run(string exeFileName, string arguments)
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

        var result = new ProcessResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = standardOutput.Any() ? standardOutput : null,
            StandardError = standardOutput.Any() ? standardError : null
        };

        if (result.ExitCode > 0)
            throw new ApplicationException($"Process finished with exit code {result.ExitCode}: {result.StandardError + result.StandardOutput}");

        return result;
    }

    public async Task RunAsync(string exeFileName, string arguments)
    {
        await new Task(() => Run(exeFileName, arguments));
    }
}

public class ProcessResult
{
    public int ExitCode { get; set; }

    public string StandardOutput { get; set; }

    public string StandardError { get; set; }
}