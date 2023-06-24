namespace CommandSurfacer.Services;

public interface ISendHelpMessages
{
    public void SendClientHelp();
    public void SendClientHelp(GroupAttribute groupAttribute);
    public void SendClientHelp(SurfaceAttribute surfaceAttribute);
}
