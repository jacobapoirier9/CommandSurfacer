namespace CommandSurfacer.Services;

public class CommandSurfacerHelp
{
    public List<CommandSurface> MethodLevelIdentifiedSurfaces { get; set; }

    public List<IGrouping<SurfaceAttribute, CommandSurface>> TypeLevelIdentifiedSurfaces { get; set; }
}

public class SurfaceHelp
{
    public SurfaceAttribute Attribute { get; set; }
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

    [Surface("help")]
    public void DisplayHelpMenu()
    {
        var help = CreateCommandSurfacerHelp();

        foreach (var surface in help.MethodLevelIdentifiedSurfaces)
        {
            Console.WriteLine(surface.MethodAttribute.Name + " - " + surface.MethodAttribute.HelpText);
        }

        foreach (var group in help.TypeLevelIdentifiedSurfaces)
        {
            Console.WriteLine(group.Key.Name + " - " + group.Key.HelpText);
            foreach (var surface in group)
            {
                Console.WriteLine(surface.MethodAttribute.Name + " - " + surface.MethodAttribute.HelpText);
            }
        }
    }
}
