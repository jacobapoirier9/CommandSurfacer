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
            {   typeof(bool), (input) =>
                {
                    if (allowedTrueValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return true;
                    else if (allowedFalseValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return false;

                    return Activator.CreateInstance<bool>();
                }
            },
            {
                typeof(bool?), (input) =>
                {
                    if (allowedTrueValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return true;
                    else if (allowedFalseValues.Contains(input, StringComparer.OrdinalIgnoreCase))
                        return false;

                    return Activator.CreateInstance<bool?>();
                }
            },
            { typeof(string), (input) => input },
            { typeof(char), (input) => char.Parse(input) },
            { typeof(char?), (input) => char.TryParse(input, out var outValue) ? outValue : null },
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

            { typeof(DateTime), (input) => DateTime.Parse(input) },
            { typeof(DateTime?), (input) => DateTime.TryParse(input, out var outValue) ? outValue : null },
            { typeof(TimeSpan), (input) => TimeSpan.Parse(input) },
            { typeof(TimeSpan?), (input) => TimeSpan.TryParse(input, out var outValue) ? outValue : null },

            { typeof(FileInfo), (input) => new FileInfo(input) },
            { typeof(DirectoryInfo), (input) => new DirectoryInfo(input) }
        };
    }

    public bool SupportsType(Type targetType) => _converters.Keys.Contains(targetType);

    public object Convert(Type targetType, string input = null)
    {
        if (_converters.TryGetValue(targetType, out var converter))
        {
            try
            {
                return converter(input);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Faled converting string '{input}' to type {targetType}", ex);
            }
        }

        throw new InvalidOperationException($"Failed converting string to {targetType}. Not supported.");
    }
}
