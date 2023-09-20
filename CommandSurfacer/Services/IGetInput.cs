namespace CommandSurfacer.Services;

public interface IGetInput
{
    public T GetInput<T>(string prompt);
    public object GetInput(string prompt, Type targetType);
    public string GetInput(string prompt);
}
