namespace CommandSurfacer;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Class)]
public class SurfaceAttribute : Attribute
{
    public string Name { get; private init; }

    public SurfaceAttribute(string name)
    {
        Name = name;
    }

    public string HelpText { get; init; }
}