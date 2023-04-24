using System.ComponentModel;

namespace CommandSurfacer.Services;

public class ConsoleInputProvider : IInputProvider
{
    private readonly IStringConverter _stringConverter;
    public ConsoleInputProvider(IStringConverter stringConverter)
    {
        _stringConverter = stringConverter;
    }

    public T GetResponse<T>(string prompt)
    {
        return _stringConverter.Convert<T>(GetResponse(prompt));
    }

    public object GetResponse(string prompt, Type targetType)
    {
        return _stringConverter.Convert(targetType, GetResponse(prompt));
    }

    public string GetResponse(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }
}
