using CommandSurfacer.Services;
using System.Text.RegularExpressions;

namespace CommandSurfacer.ConsoleApp;

public class AppCmdService
{
    private static string _appCmd = @$"{Environment.GetEnvironmentVariable("WinDir")}\System32\inetsrv\appcmd.exe";

    private readonly IProcessService _processService;
    private readonly IStringConverter _stringConverter;
    public AppCmdService(IProcessService processService, IStringConverter stringConverter)
    {
        _processService = processService;
        _stringConverter = stringConverter;
    }

    private async Task<string[]> RunCommandAsync(string command)
    {
        var result = await _processService.RunAsync(_appCmd, command);
        var split = result.StandardOutputString.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        return split;
    }

    public async Task<IEnumerable<object>> GetLoggingDirectoriesAsync()
    {
        var sites = (await RunCommandAsync("list site"))
            .Select(line =>
            {
                var match = Regex.Match(line, @"SITE ""(?<SiteName>[^""]*)""\s\(id:(?<SiteId>\d*),(bindings:(?<Bindings>.*),state:(?<State>\w*))\)");
                return new
                {
                    Id = _stringConverter.Convert<int>(match.Groups["SiteId"].Value),
                    Name = match.Groups["SiteName"].Value,
                    Bindings = match.Groups["Bindings"].Value.Split(','),
                    State = match.Groups["State"].Value
                };
            });

        var logDirectories = (await RunCommandAsync("list site /text:logFile.directory"));

        var joined = Helpers.JoinOnIndex(sites, logDirectories, (site, directory) => new
        {
            site.Id,
            site.Name,
            LogDirectory = new DirectoryInfo(Path.Combine(directory, $"W3SVC{site.Id}"))
        }).ToList();

        return joined;
    }
}
