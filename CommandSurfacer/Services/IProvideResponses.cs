namespace CommandSurfacer.Services;

public interface IProvideResponses
{
    public T GetResponse<T>(string prompt);
    public object GetResponse(string prompt, Type targetType);
    public string GetResponse(string prompt);
}
