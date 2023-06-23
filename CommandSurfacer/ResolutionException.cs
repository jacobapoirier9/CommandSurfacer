namespace CommandSurfacer;

public class ResolutionException : Exception
{
    public int SurfacesFound { get; private init; }

    public int SurfacesAvailable { get; private init; }

    public ResolutionException(int surfacesFound, int surfacesAvailable) : base($"Found {surfacesFound} out of {surfacesAvailable} surfaces to execute.")
    {
        SurfacesFound = surfacesFound;
        SurfacesAvailable = surfacesAvailable;
    }
}