namespace CommandSurfacer.Services;

public interface IStringEnumerableConverter
{
    public Type GetUnderlyingType(Type targetType);
    public bool SupportsType(Type targetType);
    public object Convert(IEnumerable<string> input, Type targetType);
}
