namespace CommandSurfacer.Services;

public interface IStringConverter
{
    public T Convert<T>(string input = null) => (T)Convert(typeof(T), input);
    public object Convert(Type targetType, string input = null);

    public bool SupportsType<T>() => SupportsType(typeof(T));
    public bool SupportsType(Type targetType);
}
