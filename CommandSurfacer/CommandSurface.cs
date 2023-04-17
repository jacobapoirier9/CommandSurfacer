using System.Reflection;

namespace CommandSurfacer;

public class CommandSurface
{
    public Type Type { get; set; }

    public SurfaceAttribute TypeAttribute { get; set; }

    public MethodInfo Method { get; set; }

    public SurfaceAttribute MethodAttribute { get; set; }
}
