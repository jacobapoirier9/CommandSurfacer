namespace CommandSurfacer.Services;

public class GetConsoleInput : IGetInput
{
    private readonly IStringConverter _stringConverter;
    public GetConsoleInput(IStringConverter stringConverter)
    {
        _stringConverter = stringConverter;
    }

    public T GetInput<T>(string prompt)
    {
        return _stringConverter.Convert<T>(GetInput(prompt));
    }

    public object GetInput(string prompt, Type targetType)
    {
        return _stringConverter.Convert(targetType, GetInput(prompt));
    }

    public string GetInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }
}
