namespace CommandSurfacer;

[AttributeUsage(AttributeTargets.Class)]
public class GroupAttribute : SurfaceAttribute
{
    public GroupAttribute(string name) : base(name) { }
}