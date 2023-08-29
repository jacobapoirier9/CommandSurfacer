using CommandSurfacer.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CommandSurfacer.Services;

public class ArgsParser : IArgsParser
{
    private readonly List<CommandSurface> _commandSurfaces;
    private readonly IStringConverter _stringConverter;
    private readonly IServiceProvider _serviceProvider;
    private readonly IStringEnumerableConverter _stringEnumerableConverter;
    private readonly CliOptions _options;

    private readonly Regex _commandSurfaceRegex;

    private readonly Dictionary<Type, Func<object>> _getSpecialValues =
        new Dictionary<Type, Func<object>>()
        {
            { typeof(TextReader), () => Console.In },
            { typeof(TextWriter), () => Console.Out },
        };

    public ArgsParser(
        List<CommandSurface> commandSurfaces, 
        IStringConverter stringConverter, 
        IStringEnumerableConverter stringEnumerableConverter, 
        IServiceProvider serviceProvider,
        CliOptions options)
    {
        _commandSurfaces = commandSurfaces;
        _stringConverter = stringConverter;
        _stringEnumerableConverter = stringEnumerableConverter;
        _serviceProvider = serviceProvider;
        _options = options;

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
    private static string ToRegexPattern(IEnumerable<string> values) => string.Join('|', values.Where(s => s is not null).OrderByDescending(s => s.Length).Select(Regex.Escape));
    private static string ToRegexPattern(params string[] values) => ToRegexPattern(values.Cast<string>());

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
            throw new ApplicationException($"Found {results.Count} out of {_commandSurfaces.Count} surfaces to execute.");

        return results.First();
    }

    public bool? ParsePresenceValue(ref string input, Type targetType, SurfaceAttribute surfaceAttribute = null)
    {
        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>name)(?<Separator>[ :=]+(?<Value>false|true|yes|no|y|n|1|0)? *|$)
        var pattern = @$"(?<= |^) *(?<Prefix>{ToRegexPattern(_options.SwitchPrefixes)})(?<Name>{ToRegexPattern(surfaceAttribute.Name, surfaceAttribute.Alias)})(?<Separator>[ :=]+(?<Value>{ToRegexPattern(_options.ConvertStringsToBool)})? *|$)";
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
        // TOOD: Use \k<Quote> backreference
        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>name)(?<Separator>[ :=]+)(?!--|-|\/)(?<Value>[^ "']+|"[^"]*"|'[^']*') *|$
        var pattern = $@"(?<= |^) *(?<Prefix>{ToRegexPattern(_options.SwitchPrefixes)})(?<Name>{ToRegexPattern(surfaceAttribute.Name, surfaceAttribute.Alias)})(?<Separator>[ :=]+)(?!{ToRegexPattern(_options.SwitchPrefixes)})(?<Value>[^ ""']+|""[^""]*""|'[^']*') *|$";
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


    // This is a cheeky work around to parse an input value, as regex has proven extremely difficult for this piece.
    public IEnumerable<string> ParseEnumerableValue(ref string input, SurfaceAttribute surfaceAttribute = null)
    {
        var pattern = @$"({ToRegexPattern(_options.SwitchPrefixes)}){ToRegexPattern(surfaceAttribute.Name, surfaceAttribute.Alias)}";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        var match = regex.Match(input);

        if (!match.Success)
            return default;

        var switchName = match.Value;
        var index = input.IndexOf(switchName);

        var list = new List<string>();

        // Enter an infinite loop and call ParseStringValue until it returns a null value (which means there are no more values provided).
        while (true)
        {
            var value = ParseStringValue(ref input, surfaceAttribute);
            if (string.IsNullOrEmpty(value))
                break;

            // Insert the switch name to its original position, as ParseStringValue removes it. This allows ParseStringValue to be called again.
            input = input.Insert(index, switchName + " ");
            list.Add(value);
        }

        // Remove the switch name one last time, and do not add it back.
        ParsePresenceValue(ref input, typeof(bool), surfaceAttribute);

        return list;

        // FLAW: Pattern will return if a space is followed by a command prefix, even in a quoted string.
        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>name)(?<Separator>[ :=]+)(?<RawValue>((?! (--|-|\/)).)*)

        // FLAW: Pattern will return if more than one space is followed by a command prefix even in a quoted string.
        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>name)(?<Separator>[ :=]+)(?<RawValue>((?!(?<=["' ]) (--|-|\/)).)*)

        // FLAWLESS (At the moment)
        // FLAW: If a string does not have any quotes at all, it will pick up the terminating string after.
        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>name)(?<Separator>[ :=]+)(?<RawValue>((?!(?<=["' ]) (--|-|\/)).)*)
        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>name)(?<Separator>[ :=]+)(?<RawValue>((?!(?<=["']) * (--|-|\/)).)*)

        // Chat GPT Options (None of which are great)
        // Best Chat GPT: (?<=\s|^)(?<Prefix>--|-|\/)(?<Name>\w+)(?<Separator>[ :=]+)(?:(?<QuotedValue>'[^']*'|"[^"]*")|(?<UnquotedValue>[^"'\s]+))
        // "(?:\\.|[^"])*"|'(?:\\.|[^'])*'|(?!foo)[^"'f]+|f(?!oo)
        // "(?:\\.|[^"])*"|'(?:\\.|[^'])*'|-[^"-]*

        //var pattern = $@"(?<= |^) *(?<Prefix>{ToRegexPattern(commandPrefixes)})(?<Name>{ToRegexPattern(surfaceAttribute.Name, surfaceAttribute.Alias)})(?<Separator>[ :=]+)(?<RawValue>((?!(?<=[""']) * ({ToRegexPattern(commandPrefixes)})).)*)";
        //var regex = new Regex(pattern, RegexOptions.IgnoreCase);
        //var match = regex.Match(input);

        //if (match.Success)
        //{
        //    input = regex.Replace(input, m => string.Empty).Trim(' ');

        //    var rawValue = match.Groups["RawValue"].Value;

        //    var values = ParseRemainingStringValues(ref rawValue);
        //    return values;
        //}

        //return default;
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

        else if (_stringEnumerableConverter.SupportsType(targetType))
        {
            var parsed = ParseEnumerableValue(ref input, surfaceAttribute);
            return _stringEnumerableConverter.Convert(parsed, targetType);
        }
                
        var instance = Activator.CreateInstance(targetType);
        BindArgs(ref input, instance, targetType);

        return instance;
    }

    private object BindArgs(ref string input, object instance, Type targetType)
    {
        var properties = targetType.GetProperties()
            .Where(property => property.DeclaringType.Namespace == targetType.Namespace)
            .ToList();

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

                value = GetSpecialValue(parameter.ParameterType);
                if (value is not null)
                    BindArgs(ref input, value, parameter.ParameterType);
                else
                    value = ParseTypedValue(ref input, parameter.ParameterType, surfaceAttribute);

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

        var remainingStrings = ParseRemainingStringValues(ref input).ToList();
        if (!remainingStrings.Any())
            return response.ToArray();

        for (var i = 0; i < response.Count; i++)
        {
            if (response[i] is not null)
                continue;

            var value = remainingStrings.FirstOrDefault();
            if (value is not null)
            {
                try
                {
                    response[i] = _stringConverter.Convert(parameters[i].ParameterType, value);
                    remainingStrings.RemoveAt(0);
                }
                catch (Exception) // If the value can not be converted, it was most likely not intended to be picked up in this position.
                { }
            }
        }

        if (!remainingStrings.Any())
            return response.ToArray();

        for (var i = 0; i < response.Count; i++)
        {
            var parameter = parameters[i];
            var value = response[i];

            if (value is null && _stringEnumerableConverter.SupportsType(parameter.ParameterType))
            {
                var underlyingType = _stringEnumerableConverter.GetUnderlyingType(parameter.ParameterType);

                if (_stringConverter.SupportsType(underlyingType))
                {
                    response[i] = _stringEnumerableConverter.Convert(remainingStrings, parameter.ParameterType);
                    break; // Do not allow the parser to bind more than one anonymous enumerable. 
                }
            }
        }

        return response.ToArray();
    }

    private IEnumerable<string> ParseRemainingStringValues(ref string input)
    {
        var pattern = "(?!,)([^ ,\"']+|\"[^\"]*\"|'[^']*')";
        var regex = new Regex(pattern);
        var matches = regex.Matches(input).Cast<Match>();

        if (matches.All(m => m.Success))
        {
            input = regex.Replace(input, m => string.Empty).Trim(' ');

            var values = matches.Select(m =>
            {
                var value = m.Value;

                if (
                    (value.StartsWith('"') && value.EndsWith('"')) ||
                    (value.StartsWith("'") && value.EndsWith("'"))
                )
                    value = value.Substring(1, value.Length - 2);

                return value;
            });

            return values;
        }

        throw new ApplicationException();
    }
}