using CommandSurfacer.Models;
using System.Reflection;
using System.Text;

namespace CommandSurfacer.Services;

public class SendConsoleHelpMessages : ISendHelpMessages
{
    private readonly List<CommandSurface> _commandSurfaces;

    private readonly IStringConverter _stringConverter;
    private readonly IServiceProvider _serviceProvider;

    public SendConsoleHelpMessages(List<CommandSurface> commandSurfaces, IStringConverter stringConverter, IServiceProvider serviceProvider)
    {
        _commandSurfaces = commandSurfaces;

        _stringConverter = stringConverter;
        _serviceProvider = serviceProvider;
    }

    private CommandSurfacerHelp CreateCommandSurfacerHelp()
    {
        var methodLevel = _commandSurfaces.Where(cs => cs.TypeAttribute is null && !cs.MethodAttribute.ExcludeFromHelp.IsTrue())
            .OrderByDescending(cs => cs.MethodAttribute.Name)
            .ToList();

        var typeLevel = _commandSurfaces.Where(cs => cs.TypeAttribute is not null && !cs.TypeAttribute.ExcludeFromHelp.IsTrue())
            .OrderByDescending(cs => cs.TypeAttribute.Name)
            .ThenByDescending(cs => cs.MethodAttribute.Name)
            .GroupBy(cs => cs.TypeAttribute)
            .ToList();

        var result = new CommandSurfacerHelp
        {
            MethodLevelIdentifiedSurfaces = methodLevel,
            TypeLevelIdentifiedSurfaces = typeLevel
        };

        return result;
    }

    private void AddCommandSurfaceParameterHelp(StringBuilder builder, CommandSurface surface, string paddedSpacing)
    {
        var parameters = surface.Method.GetParameters();
        foreach (var parameter in parameters)
        {
            builder.Append(paddedSpacing);
            builder.Append("  ");

            var parameterAttribute = parameter.GetCustomAttribute<SurfaceAttribute>();
            builder.Append(parameterAttribute?.Name ?? parameter.Name);

            if (parameterAttribute is not null && !string.IsNullOrEmpty(parameterAttribute.HelpText))
            {
                builder.Append("  -  ");
                builder.Append(parameterAttribute.HelpText);
            }

            var typeAttribute = parameter.ParameterType.GetCustomAttribute<SurfaceAttribute>();
            if (typeAttribute is not null && !string.IsNullOrEmpty(typeAttribute.HelpText))
            {
                builder.Append("  -  ");
                builder.Append(typeAttribute.HelpText);
            }

            builder.AppendLine();

            if (!_stringConverter.SupportsType(parameter.ParameterType) && _serviceProvider.GetService(parameter.ParameterType) is null)
            {
                var properties = parameter.ParameterType.GetProperties();
                foreach (var property in properties)
                {
                    builder.Append(paddedSpacing);
                    builder.Append("    ");

                    var propertyAttribute = property.GetCustomAttribute<SurfaceAttribute>();
                    builder.Append(propertyAttribute?.Name ?? property.Name);

                    if (propertyAttribute is not null && !string.IsNullOrEmpty(propertyAttribute.HelpText))
                    {
                        builder.Append("  -  ");
                        builder.Append(propertyAttribute.HelpText);
                    }

                    builder.AppendLine();
                }
            }
        }
    }

    [Surface("help")]
    public void SendClientHelp()
    {
        var help = CreateCommandSurfacerHelp();

        var builder = new StringBuilder();

        builder.AppendLine();

        foreach (var surface in help.MethodLevelIdentifiedSurfaces)
        {
            builder.Append(surface.MethodAttribute.Name);
            if (!string.IsNullOrEmpty(surface.MethodAttribute.HelpText))
            {
                builder.Append("  -  ");
                builder.Append(surface.MethodAttribute.HelpText);
            }

            builder.AppendLine();
            AddCommandSurfaceParameterHelp(builder, surface, "");
        }

        builder.AppendLine();

        foreach (var group in help.TypeLevelIdentifiedSurfaces)
        {
            builder.Append(group.Key.Name);
            if (!string.IsNullOrEmpty(group.Key.HelpText))
            {
                builder.Append(" - ");
                builder.Append(group.Key.HelpText);
            }

            builder.AppendLine();

            foreach (var surface in group)
            {
                builder.Append("  ");
                builder.Append(surface.MethodAttribute.Name);
                if (!string.IsNullOrEmpty(surface.MethodAttribute.HelpText))
                {
                    builder.Append("  -  ");
                    builder.Append(surface.MethodAttribute.HelpText);
                }

                builder.AppendLine();
                AddCommandSurfaceParameterHelp(builder, surface, "  ");
            }

            builder.AppendLine();
        }

        var helpText = builder.ToString();
        Console.WriteLine(helpText);
    }
}
