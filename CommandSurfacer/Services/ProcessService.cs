using System.Diagnostics;
using System.Text;

namespace CommandSurfacer.Services;

public class ProcessService : IProcessService
{
    public string Run(string exeFileName, string arguments)
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

        if (process.ExitCode == 0)
        {
            var output = outputBuilder.ToString();
            return output;
        }
        else
        {
            var error = errorBuilder.ToString();
            throw new ApplicationException($"Process finished with exit code {process.ExitCode}: {error}");
        }
    }

    public async Task RunAsync(string exeFileName, string arguments)
    {
        await new Task(() => Run(exeFileName, arguments));
    }
}