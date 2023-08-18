namespace CommandSurfacer.Models;

public class CliOptions
{
    public List<string> SwitchPrefixes { get; set; }

    public List<string> ConvertStringsToTrue { get; set; }

    public List<string> ConvertStringsToFalse { get; set; }

    public List<string> ConvertStringsToBool => ConvertStringsToTrue.Union(ConvertStringsToFalse).ToList();
}