﻿namespace CommandSurfacer.Services;

public interface IInteractiveConsole
{
    public void BeginInteractiveMode();

    public Task BeginInteractiveModeAsync();

    public void EndInteractiveMode();
}
