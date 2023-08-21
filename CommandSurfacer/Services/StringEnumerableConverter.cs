using System.Collections;
using System.Data;

namespace CommandSurfacer.Services;

public class StringEnumerableConverter : IStringEnumerableConverter
{
    private readonly IStringConverter _stringConverter;
    public StringEnumerableConverter(IStringConverter stringConverter)
    {
        _stringConverter = stringConverter;
    }

    public Type GetUnderlyingType(Type targetType) => targetType.GetElementType() ?? targetType.GetGenericArguments().SingleOrDefault();

    public bool SupportsType(Type targetType)
    {
        if (targetType.IsAssignableTo(typeof(IEnumerable)))
        {
            var underlyingType = GetUnderlyingType(targetType);
            return _stringConverter.SupportsType(underlyingType);
        }

        return false;
    }

    public object Convert(IEnumerable<string> input, Type targetType)
    {
        if (input is null)
            return null;

        var underlyingType = GetUnderlyingType(targetType);
        var enumerable = input.Select(value => _stringConverter.Convert(underlyingType, value));

        if (targetType.IsAssignableTo(typeof(Array)))
        {
            var array = Array.CreateInstance(underlyingType, enumerable.Count());

            for (var i = 0; i < array.Length; i++)
                array.SetValue(enumerable.ElementAt(i), i);

            return array;
        }
        else
        {
            var listType = typeof(IList<>).MakeGenericType(underlyingType);
            if (targetType.IsAssignableTo(listType))
            {
                var list = Activator.CreateInstance(targetType) as IList;
                foreach (var item in enumerable)
                    list.Add(item);

                return list;
            }

            throw new NotImplementedException();
        }
    }
}