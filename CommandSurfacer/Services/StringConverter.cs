namespace CommandSurfacer.Services;

public class StringConverter : IStringConverter
{
    private readonly Dictionary<Type, Func<string, object>> _converters;

    public StringConverter()
    {
        var allowedTrueValues = new string[] { "true", "yes", "y", "1" };
        var allowedFalseValues = new string[] { "false", "no", "n", "0" };

        _converters = new Dictionary<Type, Func<string, object>>()
        {
            {
                typeof(bool), (input) =>
                {
                    if (allowedTrueValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return true;
                    else if (allowedFalseValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return false;

                    throw new NotImplementedException("If the regex did not match, this should never be firing");
                }
            },
            {
                typeof(bool?), (input) =>
                {
                    if (allowedTrueValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return true;
                    else if (allowedFalseValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return false;

                    throw new NotImplementedException("If the regex did not match, this should never be firing");
                }
            },
            { typeof(string), (input) => input },
            { typeof(byte), (input) => byte.Parse(input) },
            { typeof(byte?), (input) => byte.TryParse(input, out var outValue) ? outValue : null },
            { typeof(short), (input) => short.Parse(input) },
            { typeof(short?), (input) => short.TryParse(input, out var outValue) ? outValue : null },
            { typeof(int), (input) => int.Parse(input) },
            { typeof(int?), (input) => int.TryParse(input, out var outValue) ? outValue : null },
            { typeof(long), (input) => long.Parse(input) },
            { typeof(long?), (input) => long.TryParse(input, out var outValue) ? outValue : null },
            { typeof(double), (input) => double.Parse(input) },
            { typeof(double?), (input) => double.TryParse(input, out var outValue) ? outValue : null },
            { typeof(float), (input) => float.Parse(input) },
            { typeof(float?), (input) => float.TryParse(input, out var outValue) ? outValue : null },
            { typeof(decimal), (input) => decimal.Parse(input) },
            { typeof(decimal?), (input) => decimal.TryParse(input, out var outValue) ? outValue : null },
            { typeof(FileInfo), (input) => new FileInfo(input) },
            { typeof(DirectoryInfo), (input) => new DirectoryInfo(input) }
        };
    }

    public bool SupportsType(Type targetType) => _converters.Keys.Contains(targetType);

    public object Convert(Type targetType, string input = null) => 
        _converters.TryGetValue(targetType, out var converter) ? 
            converter(input) :
            throw new ApplicationException("Converting string to " + targetType.Name + " is not supported.");
}
