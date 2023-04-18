﻿using System.Reflection;
using System.Text.RegularExpressions;

namespace CommandSurfacer.Services;

public class ArgsParser : IArgsParser
{
    private readonly List<CommandSurface> _commandSurfaces;
    private readonly IStringConverter _stringConverter;
    private readonly IServiceProvider _serviceProvider;

    private Regex _commandSurfaceRegex;

    public ArgsParser(List<CommandSurface> commandSurfaces, IStringConverter stringConverter, IServiceProvider serviceProvider)
    {
        _commandSurfaces = commandSurfaces;
        _stringConverter = stringConverter;
        _serviceProvider = serviceProvider;

        BuildCommandSurfaceRegularExpression();
    }


    private void BuildCommandSurfaceRegularExpression()
    {
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

    public bool? ParsePresenceValue(ref string input, string targetName, Type targetType, SurfaceAttribute surfaceAttribute = null)
    {
        var allowedTrueValues = new string[] { "true", "yes", "y", "1" };
        var allowedFalseValues = new string[] { "false", "no", "n", "0" };

        var allowedBooleanValues = allowedTrueValues.Concat(allowedFalseValues);
        var allowedBooleanValuesPattern = string.Join('|', allowedBooleanValues.OrderByDescending(s => s.Length));

        var commandPrefixes = new string[] { "--", "/" };
        var commandPrefixesPattern = Regex.Escape(string.Join('|', commandPrefixes.OrderByDescending(s => s.Length)));

        var regex = new Regex($@"(?<Prefix>{commandPrefixesPattern})(?<ArgumentName>{targetName})(?<ArgumentNameTerminator>[\s:=]+(?<ArgumentValue>{string.Join('|', allowedBooleanValues)})?|$)", RegexOptions.IgnoreCase);
        var match = regex.Match(input);

        if (match.Success)
        {
            input = regex.Replace(input, m => string.Empty);

            var group = match.Groups["ArgumentValue"];

            if (group.Success)
                return _stringConverter.Convert<bool>(group.Value); // Argument value was present, and should already be in a valid true/false string format.
            else
                return true; // Argument name was present, but the argument value was not present.
        }

        return Activator.CreateInstance(targetType) as bool?; // Argument name was not present, and should return the default value.
    }

    public string ParseStringValue(ref string input, string targetName, SurfaceAttribute surfaceAttribute = null)
    {
        var commandPrefixes = new string[] { "--", "/" };
        var commandPrefixesPattern = string.Join('|', commandPrefixes.OrderByDescending(s => s.Length).Select(s => Regex.Escape(s)));

        var regex = new Regex($@"(?<Prefix>{commandPrefixesPattern})(?<ArgumentName>{targetName})(?<ArgumentNameTerminator>[\s:=]+)(?<ArgumentValue>[\w:\\.-{{}}]+|""[\w\s:\\.-{{}}]*""|'[\w\s:\\.-{{}}]*')", RegexOptions.IgnoreCase);
        var match = regex.Match(input);

        if (match.Success)
        {
            input = regex.Replace(input, m => string.Empty);

            var group = match.Groups["ArgumentValue"];
            var stringValue = group.Value.Trim('\'', '"', ' ');
            return stringValue;
        }

        return null;
    }

    public object ParseTypedValue(ref string input, string targetName, Type targetType, SurfaceAttribute surfaceAttribute = null)
    {
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            var presenceValue = ParsePresenceValue(ref input, targetName, targetType, surfaceAttribute);
            return presenceValue;
        }

        var stringValue = ParseStringValue(ref input, targetName);

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
                    var attribute = property.GetCustomAttribute<SurfaceAttribute>();
                    var value = ParseTypedValue(ref input, attribute?.Name ?? property.Name, property.PropertyType, attribute);
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
            var surfaceAttribute = parameter.GetCustomAttribute<SurfaceAttribute>();
            var value = ParseTypedValue(ref input, parameter.Name, parameter.ParameterType, surfaceAttribute);

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
