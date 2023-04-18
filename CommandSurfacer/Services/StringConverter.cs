namespace CommandSurfacer.Services;

public class StringConverter : IStringConverter
{
    public T Convert<T>(string input = null) => (T)Convert(typeof(T), input);
    public object Convert(Type targetType, string input = null)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var converted = default(object);

        if (targetType == typeof(string))
            converted = input;

        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            var lower = input.ToLower();

            if (new string[] { "true", "yes", "y", "1" }.Contains(lower))
                converted = true;
            else if (new string[] { "false", "no", "n", "0" }.Contains(lower))
                converted = false;
            else
                return Activator.CreateInstance(targetType);
        }

        else if (targetType == typeof(byte))
            converted = byte.Parse(input);
        else if (targetType == typeof(byte?))
            converted = byte.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(short))
            converted = short.Parse(input);
        else if (targetType == typeof(short?))
            converted = short.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(int))
            converted = int.Parse(input);
        else if (targetType == typeof(int?))
            converted = int.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(long))
            converted = long.Parse(input);
        else if (targetType == typeof(long?))
            converted = long.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(double))
            converted = double.Parse(input);
        else if (targetType == typeof(double?))
            converted = double.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(float))
            converted = float.Parse(input);
        else if (targetType == typeof(float?))
            converted = float.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(decimal))
            converted = decimal.Parse(input);
        else if (targetType == typeof(decimal?))
            converted = decimal.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(TimeSpan))
            converted = TimeSpan.Parse(input);
        else if (targetType == typeof(TimeSpan?))
            converted = TimeSpan.TryParse(input, out var outValue) ? outValue : null;

        else if (targetType == typeof(DateTime))
            converted = DateTime.Parse(input);
        else if (targetType == typeof(DateTime?))
            converted = DateTime.TryParse(input, out var outValue) ? outValue : null;

        if (targetType == typeof(FileInfo))
        {
            var filePath = Convert<string>(input);
            if (filePath is not null)
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo;
            }
        }
        else if (targetType == typeof(DirectoryInfo))
        {
            var directoryPath = Convert<string>(input);
            if (directoryPath is not null)
            {
                var directoryInfo = new DirectoryInfo(directoryPath);
                return directoryInfo;
            }
        }

        if (converted == default)
            throw new ApplicationException("Converting string to " + targetType.Name + " is not supported.");

        return converted;
    }
}
