namespace CommandSurfacer;

public class SurfaceAttribute : Attribute
{
    public string Name { get; private init; }

    public SurfaceAttribute(string name)
    {
        Name = name;
    }
}