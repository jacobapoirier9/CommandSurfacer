namespace CommandSurfacer;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property)]
public class SurfaceAttribute : Attribute
{
    public string Name { get; private init; }

    public string Alias { get; private init; }

    public SurfaceAttribute(string name, string alias = null)
    {
        Name = name;
        Alias = alias;
    }

    public string HelpText { get; init; }

    public bool ExcludeFromHelp { get; set; }
}