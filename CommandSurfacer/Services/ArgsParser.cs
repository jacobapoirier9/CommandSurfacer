using System.Reflection;
using System.Text.RegularExpressions;

namespace CommandSurfacer.Services;

public class ArgsParser : IArgsParser
{
    private readonly List<CommandSurface> _commandSurfaces;
    private readonly IStringConverter _stringConverter;
    private readonly IServiceProvider _serviceProvider;

    private readonly Regex _commandSurfaceRegex;

    public ArgsParser(List<CommandSurface> commandSurfaces, IStringConverter stringConverter, IServiceProvider serviceProvider)
    {
        _commandSurfaces = commandSurfaces;
        _stringConverter = stringConverter;
        _serviceProvider = serviceProvider;

        var optionalTypeSurfaceIdentifiers = _commandSurfaces.Select(cs => cs.TypeAttribute?.Name)
            .Where(cs => cs is not null)
            .OrderByDescending(cs => cs.Length)
            .ToList();

        var optionalMethodSurfaceIdentifiers = _commandSurfaces.Select(cs => cs.MethodAttribute?.Name)
            .Where(cs => cs is not null)
            .OrderByDescending(cs => cs.Length)
            .ToList();

        _commandSurfaceRegex = new Regex($"^(?<TypeIdentifier>{string.Join('|', optionalTypeSurfaceIdentifiers)})? *(?<MethodIdentifier>{string.Join('|', optionalMethodSurfaceIdentifiers)})? *", RegexOptions.IgnoreCase);
    }

    public CommandSurface ParseCommandSurface(ref string input)
    {
        var match = _commandSurfaceRegex.Match(input);

        if (!match.Success) // TODO: This check is likely not necessary.
            throw new ApplicationException($"Unexpected error in {nameof(ParseCommandSurface)}.");

        input = _commandSurfaceRegex.Replace(input, m => string.Empty);

        var typeIdentifier = match.Groups["TypeIdentifier"].Value;
        var methodIdentifier = match.Groups["MethodIdentifier"].Value;

        var filtered = _commandSurfaces.Where(cs => true); // Create a separate IEnumerable and leave the original list alone.

        if (!string.IsNullOrEmpty(typeIdentifier))
            filtered = filtered.Where(cs => cs.TypeAttribute is not null && string.Equals(cs.TypeAttribute.Name, typeIdentifier, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(methodIdentifier))
            filtered = filtered.Where(cs => cs.MethodAttribute is not null && string.Equals(cs.MethodAttribute.Name, methodIdentifier, StringComparison.OrdinalIgnoreCase));

        var results = filtered.ToList();

        if (results.Count == 0)
            throw new ApplicationException("Could not resolve any command surfaces.");
        else if (results.Count > 1)
            throw new ApplicationException("Could not resolve 1 command surface out of " + results.Count + " canditates.");

        return results.First();
    }

    public bool? ParsePresenceValue(ref string input, SurfaceAttribute surfaceAttribute, Type targetType)
    {
        var allowedTrueValues = new string[] { "true", "yes", "y", "1" };
        var allowedFalseValues = new string[] { "false", "no", "n", "0" };

        var allowedBooleanValues = allowedTrueValues.Concat(allowedFalseValues);
        var allowedBooleanValuesPattern = string.Join('|', allowedBooleanValues.OrderByDescending(s => s.Length));

        var commandPrefixes = new string[] { "--", "-", "/" };
        var commandPrefixesPattern = string.Join('|', commandPrefixes.OrderByDescending(s => s.Length).Select(s => Regex.Escape(s)));

        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>test-name)(?<Separator>[ :=]+(?<Value>false|true|yes|no|y|n|1|0)? *|$)
        var pattern = @$"(?<= |^) *(?<Prefix>{commandPrefixesPattern})(?<Name>{surfaceAttribute.Name})(?<Separator>[ :=]+(?<Value>{allowedBooleanValuesPattern})? *|$)";
        var  regex = new Regex(pattern, RegexOptions.IgnoreCase);
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
        var commandPrefixesPattern = string.Join('|', commandPrefixes.OrderByDescending(s => s.Length).Select(s => Regex.Escape(s)));

        // (?<= |^) *(?<Prefix>--|-|\/)(?<Name>test-name)(?<Separator>[ :=]+)(?<Value>[\w:\\.-{}]+|"[\w\s:\\.-{}',]*"|'[\w\s:\\.-{}",]*') *|$
        var pattern = $@"(?<= |^) *(?<Prefix>{commandPrefixesPattern})(?<Name>{surfaceAttribute.Name})(?<Separator>[ :=]+)(?<Value>[\w:\\.-{{}}]+|""[\w\s:\\.-{{}}',]*""|'[\w\s:\\.-{{}}"",]*') *|$";
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

    public object ParseTypedValue(ref string input, SurfaceAttribute surfaceAttribute, Type targetType)
    {
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            var presenceValue = ParsePresenceValue(ref input, surfaceAttribute, targetType);
            return presenceValue;
        }

        var stringValue = ParseStringValue(ref input, surfaceAttribute);

        if (stringValue is not null)
        {
            var typedValue = _stringConverter.Convert(targetType, stringValue);
            return typedValue;
        }
        else
        {
            var injectedService = _serviceProvider.GetService(targetType);
            if (injectedService is not null)
                return injectedService;

            try
            {
                var instance = Activator.CreateInstance(targetType);
                
                var properties = targetType.GetProperties();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<SurfaceAttribute>() ?? new SurfaceAttribute(property.Name);
                    var value = ParseTypedValue(ref input, surfaceAttribute, property.PropertyType);
                    property.SetValue(instance, value);
                }

                return instance;
            }
            catch
            {
                return null;
            }
        }
    }

    public object[] ParseMethodParameters(ref string input, MethodInfo method, params object[] additionalParameters)
    {
        var response = new List<object>();

        var parameters = method.GetParameters();
        foreach (var parameter in parameters)
        {
            var surfaceAttribute = parameter.GetCustomAttribute<SurfaceAttribute>() ?? new SurfaceAttribute(parameter.Name);
            var value = ParseTypedValue(ref input, surfaceAttribute, parameter.ParameterType);

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

            if (value is null)
            {
                var additionalParameter = additionalParameters.FirstOrDefault(ap => ap.GetType().IsAssignableTo(parameter.ParameterType));
                if (additionalParameter is not null)
                    value = additionalParameter;
            }

            response.Add(value);
        }

        var regex = new Regex(@"(?<ArgumentValue>[\w:\\.-{{}}]+|""[\w\s:\\.-{{}}]*""|'[\w\s:\\.-{{}}]*')");
        var matches = regex.Matches(input).Cast<Match>().ToList();

        var matchesIndex = 0;
        for (var i = 0; i < response.Count; i++)
        {
            if (response[i] is not null)
                continue;

            var match = matches.ElementAtOrDefault(matchesIndex);
            if (match is not null)
            {
                try 
                { 
                    response[i] = _stringConverter.Convert(parameters[i].ParameterType, match.Value.Trim('\'', '"', ' '));
                    matchesIndex++;
                }
                catch { }
            }
        }

        return response.ToArray();
    }
}
