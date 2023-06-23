namespace CommandSurfacer.Models;

public class CommandSurfacerHelp
{
    public List<CommandSurface> MethodLevelIdentifiedSurfaces { get; set; }

    public List<IGrouping<SurfaceAttribute, CommandSurface>> TypeLevelIdentifiedSurfaces { get; set; }
}
