namespace CommandSurfacer.Services;

public class GetMemoryInput : IGetInput
{
    private readonly IEnumerator<object> _enumerator;
    public GetMemoryInput(params object[] responses) { _enumerator = (responses as IEnumerable<object>).GetEnumerator(); }


    public T GetInput<T>(string prompt) => (T)GetInput(prompt, typeof(T));

    public object GetInput(string prompt, Type targetType)
    {
        if (_enumerator.MoveNext())
        {
            var current = _enumerator.Current;
            var currentType = current.GetType();

            if (currentType != targetType)
                throw new InvalidCastException($"Expected type {targetType.Name} but got {currentType.Name}");

            return current;
        }
        else
            throw new IndexOutOfRangeException("Not enough elements provided to continue reading inputs");
    }

    public string GetInput(string prompt) => (string)GetInput(prompt, typeof(string));
}