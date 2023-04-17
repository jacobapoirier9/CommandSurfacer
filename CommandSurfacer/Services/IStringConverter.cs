namespace CommandSurfacer.Services;

public interface IStringConverter
{
    public T Convert<T>(string input = null);
    public object Convert(Type targetType, string input = null);
}
