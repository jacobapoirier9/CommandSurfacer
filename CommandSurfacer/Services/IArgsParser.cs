using System.Reflection;

namespace CommandSurfacer.Services;

public interface IArgsParser
{
    public CommandSurface ParseCommandSurface(ref string input);

    public bool? ParsePresenceValue(ref string input, string targetName, Type targetType);

    public string ParseStringValue(ref string input, string targetName);

    public object ParseTypedValue(ref string input, string targetName, Type targetType);

    public object[] ParseMethodParameters(ref string input, MethodInfo method);
}
