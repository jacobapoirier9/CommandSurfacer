namespace CommandSurfacer.Models;

public class CommandSurfacerHelp
{
    public List<CommandSurface> Surfaces { get; set; }

    public List<IGrouping<GroupAttribute, CommandSurface>> Groups { get; set; }
}
