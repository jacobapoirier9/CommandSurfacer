using System.Reflection;

namespace CommandSurfacer.Services;

public interface IArgsParser
{
    public GroupAttribute ResolveGroupAttributeOrDefault(string input);
    public SurfaceAttribute ResolveSurfaceAttributeOrDefault(string input);
    public CommandSurface ParseCommandSurface(ref string input);

    public T ParsePresenceValue<T>(ref string input, SurfaceAttribute surfaceAttribute = null) => (T)(object)ParsePresenceValue(ref input, typeof(T), surfaceAttribute);
    public bool? ParsePresenceValue(ref string input, Type targetType, SurfaceAttribute surfaceAttribute = null);

    public string ParseStringValue(ref string input, SurfaceAttribute surfaceAttribute = null);

    public T ParseTypedValue<T>(ref string input, SurfaceAttribute surfaceAttribute = null) => (T)ParseTypedValue(ref input, typeof(T), surfaceAttribute);
    public object ParseTypedValue(ref string input, Type targetType, SurfaceAttribute surfaceAttribute = null);


    public T GetSpecialValue<T>() => (T)GetSpecialValue(typeof(T));
    public object GetSpecialValue(Type targetType);

    public object[] ParseMethodParameters(ref string input, MethodInfo method, params object[] additionalParameters);
}
