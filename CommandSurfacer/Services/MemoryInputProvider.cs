namespace CommandSurfacer.Services;

public class MemoryInputProvider : IInputProvider
{
    private readonly IEnumerator<object> _enumerator;
    public required IEnumerable<object> Responses { init { _enumerator = value.GetEnumerator(); } }


    public T GetResponse<T>(string prompt) => (T)GetResponse(prompt, typeof(T));

    public object GetResponse(string prompt, Type targetType)
    {
        if (_enumerator is null)
            throw new NullReferenceException($"A value was not provided to the {nameof(Responses)} collection.");

        if (_enumerator.MoveNext())
        {
            var current = _enumerator.Current;
            var currentType = current.GetType();

            if (currentType != targetType)
                throw new ApplicationException($"Expected type {targetType.Name} but got {currentType.Name}");

            return current;
        }
        else
            throw new IndexOutOfRangeException("Not enough elements provided to continue reading inputs");
    }

    public string GetResponse(string prompt) => (string)GetResponse(prompt, typeof(string));
}