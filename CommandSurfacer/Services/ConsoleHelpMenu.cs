using System.Reflection;
using System.Text;

namespace CommandSurfacer.Services;

public class CommandSurfacerHelp
{
    public List<CommandSurface> MethodLevelIdentifiedSurfaces { get; set; }

    public List<IGrouping<SurfaceAttribute, CommandSurface>> TypeLevelIdentifiedSurfaces { get; set; }
}

public class ConsoleHelpMenu : IConsoleHelpMenu
{
    private readonly List<CommandSurface> _commandSurfaces;
    private readonly InteractiveConsoleOptions _interactiveConsoleOptions;

    public ConsoleHelpMenu(List<CommandSurface> commandSurfaces, InteractiveConsoleOptions interactiveConsoleOptions)
    {
        _commandSurfaces = commandSurfaces;
        _interactiveConsoleOptions = interactiveConsoleOptions;
    }

    private CommandSurfacerHelp CreateCommandSurfacerHelp()
    {
        var methodLevel = _commandSurfaces.Where(cs => cs.TypeAttribute is null)
            .OrderByDescending(cs => cs.MethodAttribute.Name)
            .ToList();

        var typeLevel = _commandSurfaces.Where(cs => cs.TypeAttribute is not null)
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

    public void AddCommandSurfaceParameterHelp(StringBuilder builder, CommandSurface surface)
    {
        var parameters = surface.Method.GetParameters();
        foreach (var parameter in parameters)
        {
            builder.Append("  ");

            var attribute = parameter.GetCustomAttribute<SurfaceAttribute>();
            builder.Append(attribute?.Name ?? parameter.Name);
            if (attribute is not null && !string.IsNullOrEmpty(attribute.HelpText))
            {
                builder.Append("  -  ");
                builder.Append(attribute.HelpText);
            }
            builder.AppendLine();
        }
    }

    [Surface("help")]
    public void DisplayHelpMenu()
    {
        var help = CreateCommandSurfacerHelp();

        var builder = new StringBuilder();

        builder.AppendLine(_interactiveConsoleOptions.Banner);
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
            AddCommandSurfaceParameterHelp(builder, surface);
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
                builder.Append("  ");
                AddCommandSurfaceParameterHelp(builder, surface);
            }

            builder.AppendLine();
        }

        var helpText = builder.ToString();
        Console.WriteLine(helpText);
    }
}
