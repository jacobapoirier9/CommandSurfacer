using CommandSurfacer.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace CommandSurfacer.Services;

public class SendConsoleHelpMessages : ISendHelpMessages
{
    private readonly List<CommandSurface> _commandSurfaces;

    private readonly IStringConverter _stringConverter;
    private readonly IServiceProvider _serviceProvider;

    private readonly InteractiveConsoleOptions _consoleOptions;

    public SendConsoleHelpMessages(List<CommandSurface> commandSurfaces, IStringConverter stringConverter, IServiceProvider serviceProvider)
    {
        _commandSurfaces = commandSurfaces;

        _stringConverter = stringConverter;
        _serviceProvider = serviceProvider;

        _consoleOptions = _serviceProvider.GetService<InteractiveConsoleOptions>();
    }

    private CommandSurfacerHelp CreateCommandSurfacerHelp()
    {
        var groups = _commandSurfaces.Where(cs => cs.Group is null && !cs.Surface.ExcludeFromHelp)
            .OrderByDescending(cs => cs.Surface.Name)
            .ToList();

        var surfaces = _commandSurfaces.Where(cs => cs.Group is not null && !cs.Group.ExcludeFromHelp)
            .OrderByDescending(cs => cs.Group.Name)
            .ThenByDescending(cs => cs.Surface.Name)
            .GroupBy(cs => cs.Group)
            .ToList();

        var result = new CommandSurfacerHelp
        {
            Surfaces = groups,
            Groups = surfaces
        };

        return result;
    }

    private static int CalculateMaxStringLength(params IEnumerable<string>[] collections)
    {
        var flatten = collections.SelectMany(c => c).Select(c => c.Length);
        var max = flatten.Any() ? flatten.Max() : default;
        return max;
    }

    private void AppendBanner(StringBuilder builder)
    {
        builder.AppendLine();

        if (_consoleOptions is not null && _consoleOptions.Banner is not null)
            builder.AppendLine(_consoleOptions.Banner);
    }

    private void AppendGroupLine(StringBuilder builder, GroupAttribute group, int maxNameLength)
    {
        builder.AppendLine(string.Format("    {0," + maxNameLength + "}        {1}", group.Name, group.HelpText));
    }

    private void AppendSurfaceLine(StringBuilder builder, SurfaceAttribute surface, int maxNameLength)
    {
        builder.AppendLine(string.Format("    {0," + maxNameLength + "}        {1}", surface.Name, surface.HelpText));
    }

    private void AppendClientHelp(StringBuilder builder, CommandSurfacerHelp help)
    {
        AppendBanner(builder);

        var maxNameLength = CalculateMaxStringLength(help.Groups.Select(g => g.Key.Name), help.Surfaces.Select(s => s.Surface.Name));

        foreach (var group in help.Groups)
        {
            builder.AppendLine();
            AppendGroupLine(builder, group.Key, maxNameLength);
        }

        foreach (var surface in help.Surfaces)
        {
            builder.AppendLine();
            AppendSurfaceLine(builder, surface.Surface, maxNameLength);
        }
    }

    private void AppendAllGroupHelp(StringBuilder builder, GroupAttribute groupAttribute)
    {
        AppendBanner(builder);

        var surfaces = _commandSurfaces.Where(cs => cs.Group is not null && cs.Group.Name == groupAttribute.Name);
        var maxNameLength = CalculateMaxStringLength(surfaces.Select(s => s.Surface.Name));

        builder.AppendLine();
        AppendGroupLine(builder, groupAttribute, groupAttribute.Name.Length);
        foreach (var surface in surfaces)
        {
            builder.AppendLine();
            builder.Append("  ");
            AppendSurfaceLine(builder, surface.Surface, maxNameLength);
        }
    }

    private void AppendAllSurfaceHelp(StringBuilder builder, SurfaceAttribute surfaceAttribute)
    {
        AppendBanner(builder);

        var surface = _commandSurfaces.Single(cs => /*(cs.Group is null || cs.Group.Name == surfaceAttribute.Name) &&*/ (cs.Surface.Name == surfaceAttribute.Name));

        var dictionary = new Dictionary<string, SurfaceAttribute>();

        var parameters = surface.Method.GetParameters();
        foreach (var parameter in parameters)
        {
            var parameterAttribute = parameter.GetCustomAttribute<SurfaceAttribute>() ?? new SurfaceAttribute(parameter.Name);
            if (!_stringConverter.SupportsType(parameter.ParameterType) && _serviceProvider.GetService(parameter.ParameterType) is null)
            {
                var properties = parameter.ParameterType.GetProperties();
                foreach (var property in properties)
                {
                    var propertyAttribute = property.GetCustomAttribute<SurfaceAttribute>() ?? new SurfaceAttribute(property.Name);
                    dictionary.Add(propertyAttribute.Name, propertyAttribute);
                }
            }
            else
            {
                dictionary.TryAdd(parameterAttribute.Name, parameterAttribute);
            }
        }

        var maxNameLength = CalculateMaxStringLength(dictionary.Select(d => d.Key));

        builder.AppendLine();
        AppendSurfaceLine(builder, surfaceAttribute, surfaceAttribute.Name.Length);
        foreach (var item in dictionary)
        {
            builder.AppendLine();
            builder.Append("  ");
            AppendSurfaceLine(builder, item.Value, maxNameLength);
        }
    }

    private void Send(StringBuilder builder)
    {
        var output = builder.ToString();
        Console.WriteLine(output);
    }

    public void SendClientHelp()
    {
        var builder = new StringBuilder();
        var help = CreateCommandSurfacerHelp();

        AppendClientHelp(builder, help);

        Send(builder);
    }

    public void SendClientHelp(GroupAttribute groupAttribute)
    {
        var builder = new StringBuilder();

        AppendAllGroupHelp(builder, groupAttribute);

        Send(builder);
    }

    public void SendClientHelp(SurfaceAttribute surfaceAttribute)
    {
        var builder = new StringBuilder();

        AppendAllSurfaceHelp(builder, surfaceAttribute);

        Send(builder);
    }
}
