using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CommandSurfacer.Services;

public class ArgsParser : IArgsParser
{
    private readonly List<CommandSurface> _commandSurfaces;
    private readonly IStringConverter _stringConverter;
    private readonly IServiceProvider _serviceProvider;

    private readonly Regex _commandSurfaceRegex;

    private readonly Dictionary<Type, Func<object>> _getSpecialValues =
        new Dictionary<Type, Func<object>>()
        {
            { typeof(TextReader), () => Console.In },
            { typeof(TextWriter), () => Console.Out },
        };

    public ArgsParser(List<CommandSurface> commandSurfaces, IStringConverter stringConverter, IServiceProvider serviceProvider)
    {
        _commandSurfaces = commandSurfaces;
        _stringConverter = stringConverter;
        _serviceProvider = serviceProvider;

        var groupNames = _commandSurfaces.Select(cs => cs.Group?.Name)
            .Where(cs => cs is not null)
            .OrderByDescending(cs => cs.Length)
            .ToList();

        var surfaceNames = _commandSurfaces.Select(cs => cs.Surface?.Name)
            .Where(cs => cs is not null)
            .OrderByDescending(cs => cs.Length)
            .ToList();

        var pattern = $"^(?<GroupName>{string.Join('|', groupNames)})? *(?<SurfaceName>{string.Join('|', surfaceNames)})? *";
        _commandSurfaceRegex = new Regex(pattern, RegexOptions.IgnoreCase);
    }

    private static bool EqualsIgnoreCase(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    private static string ToRegexPattern(IEnumerable<string> values) => string.Join('|', values.OrderByDescending(s => s.Length).Select(Regex.Escape));

    public GroupAttribute ResolveGroupAttributeOrDefault(string input)
    {
        var match = _commandSurfaceRegex.Match(input);

        var providedName = match.Groups["GroupName"].Value;
        var filtered = _commandSurfaces.Where(cs => cs.Group is not null && EqualsIgnoreCase(cs.Group.Name, providedName));

        var results = filtered.DistinctBy(f => f.Group.Name).Select(f => f.Group).ToList();
        return results.Count == 1 ? results[0] : default;
    }

    public SurfaceAttribute ResolveSurfaceAttributeOrDefault(string input)
    {
        var match = _commandSurfaceRegex.Match(input);

        var providedName = match.Groups["SurfaceName"].Value;
        var filtered = _commandSurfaces.Where(cs => cs.Surface is not null && EqualsIgnoreCase(cs.Surface.Name, providedName));

        var results = filtered.DistinctBy(f => f.Surface.Name).Select(f => f.Surface).ToList();
        return results.Count == 1 ? results[0] : default;
    }

    public CommandSurface ParseCommandSurface(ref string input)
    {
        var match = _commandSurfaceRegex.Match(input);
        input = _commandSurfaceRegex.Replace(input, m => string.Empty);

        var typeIdentifier = match.Groups["GroupName"].Value;
        var methodIdentifier = match.Groups["SurfaceName"].Value;

        var filtered = _commandSurfaces.Where(cs => true);

        if (!string.IsNullOrEmpty(typeIdentifier))
            filtered = filtered.Where(cs => cs.Group is not null && EqualsIgnoreCase(cs.Group.Name, typeIdentifier));

        if (!string.IsNullOrEmpty(methodIdentifier))
            filtered = filtered.Where(cs => cs.Surface is not null && EqualsIgnoreCase(cs.Surface.Name, methodIdentifier));

        var results = filtered.ToList();

        if (results.Count != 1)
            throw new ResolutionException(results.Count, _commandSurfaces.Count);

        return results.First();
    }

    public bool? ParsePresenceValue(ref string input, Type targetType, SurfaceAttribute surfaceAttribute = null)
    {
        var allowedTrueValues = new string[] { "true", "yes", "y", "1" };
        var allowedFalseValues = new string[] { "false", "no", "n", "0" };

        var allowedBooleanValues = allowedTrueValues.Concat(allowedFalseValues);
        var commandPrefixes = new string[] { "--", "-", "/" };

        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>test-name)(?<Separator>[ :=]+(?<Value>false|true|yes|no|y|n|1|0)? *|$)
        var pattern = @$"(?<= |^) *(?<Prefix>{ToRegexPattern(commandPrefixes)})(?<Name>{Regex.Escape(surfaceAttribute.Name)})(?<Separator>[ :=]+(?<Value>{ToRegexPattern(allowedBooleanValues)})? *|$)";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        var match = regex.Match(input);

        if (match.Success)
        {
            input = regex.Replace(input, m => string.Empty).Trim(' ');

            var group = match.Groups["Value"];

            if (group.Success)
                return _stringConverter.Convert<bool>(group.Value); // Argument value was present, and should already be in a valid true/false string format.
            else
                return true; // Argument name was present, but the argument value was not present.
        }

        return Activator.CreateInstance(targetType) as bool?; // Argument name was not present, and should return the default value.
    }

    public string ParseStringValue(ref string input, SurfaceAttribute surfaceAttribute)
    {
        var commandPrefixes = new string[] { "--", "-", "/" };

        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>test-name)(?<Separator>[ :=]+)(?<Value>[^ "']+|"[^"]*"|'[^']*') *|$
        var pattern = $@"(?<= |^) *(?<Prefix>{ToRegexPattern(commandPrefixes)})(?<Name>{Regex.Escape(surfaceAttribute.Name)})(?<Separator>[ :=]+)(?<Value>[^ ""']+|""[^""]*""|'[^']*') *|$";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        var match = regex.Match(input);

        if (match.Success)
        {
            input = regex.Replace(input, m => string.Empty).Trim(' ');

            var group = match.Groups["Value"];
            var stringValue = group.Value.Trim(' ');

            if (
                (stringValue.StartsWith('"') && stringValue.EndsWith('"')) ||
                (stringValue.StartsWith("'") && stringValue.EndsWith("'"))
            )
                stringValue = stringValue.Substring(1, stringValue.Length - 2);

            return string.IsNullOrEmpty(stringValue) ? null : stringValue;
        }

        return null;
    }

    public object ParseTypedValue(ref string input, Type targetType, SurfaceAttribute surfaceAttribute = null)
    {
        if (_stringConverter.SupportsType(targetType))
        {
            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                var presenceValue = ParsePresenceValue(ref input, targetType, surfaceAttribute);
                return presenceValue;
            }

            var stringValue = ParseStringValue(ref input, surfaceAttribute);
            if (stringValue is null)
                return null;
            else
                return _stringConverter.Convert(targetType, stringValue);
        }
        
        var instance = Activator.CreateInstance(targetType);
        var properties = targetType.GetProperties();
        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttribute<SurfaceAttribute>() ?? new SurfaceAttribute(property.Name);
            var value = ParseTypedValue(ref input, property.PropertyType, attribute);
            property.SetValue(instance, value);
        }

        return instance;
    }

    public object GetSpecialValue(Type targetType)
    {
        if (_getSpecialValues.TryGetValue(targetType, out var getSpecialValue))
        {
            var specialValue = getSpecialValue();
            return specialValue;
        }
        else
        {
            var injectedService = _serviceProvider.GetService(targetType);
            return injectedService;
        }
    }

    public object[] ParseMethodParameters(ref string input, MethodInfo method, params object[] additionalParameters)
    {
        var response = new List<object>();

        var additionalParametersList = additionalParameters.ToList();

        var parameters = method.GetParameters();
        foreach (var parameter in parameters)
        {
            var value = default(object);

            var additionalParameter = additionalParametersList.FirstOrDefault(ap => ap.GetType().IsAssignableTo(parameter.ParameterType));
            if (additionalParameter is not null)
            {
                value = additionalParameter;
                additionalParametersList.Remove(additionalParameter);
            }
            else
            {
                var surfaceAttribute = parameter.GetCustomAttribute<SurfaceAttribute>() ?? new SurfaceAttribute(parameter.Name);
                value = GetSpecialValue(parameter.ParameterType) ?? ParseTypedValue(ref input, parameter.ParameterType, surfaceAttribute);

                // If ParseTypedValue returns the default value, we do not want to add it to response.
                // This will allow anonymous parameters to be inserted more accurately.
                if (parameter.ParameterType.IsAssignableTo(typeof(IConvertible)))
                {
                    try
                    {
                        if (object.Equals(value, Activator.CreateInstance(parameter.ParameterType)))
                            value = null;
                    }
                    catch { }
                }
            }

            response.Add(value);
        }

        var pattern = @"(?<ArgumentValue>[^ ""']+|""[^""]*""|'[^']*')";
        var regex = new Regex(pattern);
        var matches = regex.Matches(input).Cast<Match>().ToList();

        var matchesIndex = 0;
        for (var i = 0; i < response.Count; i++)
        {
            if (response[i] is not null)
                continue;

            var match = matches.ElementAtOrDefault(matchesIndex);
            if (match is not null)
            {
                response[i] = _stringConverter.Convert(parameters[i].ParameterType, match.Value.Trim('\'', '"', ' '));
                matchesIndex++;

                input = Utils.ReplaceFirstOccurance(input, match.Value, string.Empty).Trim('\'', '"', ' ');
            }
        }

        return response.ToArray();
    }
}
