namespace CommandSurfacer.Services;

public interface IInputProvider
{
    public T GetResponse<T>(string prompt);
    public object GetResponse(string prompt, Type targetType);
    public string GetResponse(string prompt);
}
