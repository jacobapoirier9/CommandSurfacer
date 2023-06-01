using System.Reflection;

namespace CommandSurfacer.Services;

public interface IArgsParser
{
    public CommandSurface ParseCommandSurface(ref string input);
    public bool? ParsePresenceValue(ref string input, SurfaceAttribute surfaceAttribute, Type targetType);
    public string ParseStringValue(ref string input, SurfaceAttribute surfaceAttribute);
    public object ParseTypedValue(ref string input, SurfaceAttribute surfaceAttribute, Type targetType);
    public object GetSpecialValue(Type targetType);
    public object[] ParseMethodParameters(ref string input, MethodInfo method, params object[] additionalParameters);
}
