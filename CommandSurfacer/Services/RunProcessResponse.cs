﻿namespace CommandSurfacer.Services;

public class RunProcessResponse
{
    public int ExitCode { get; set; }

    public string StandardOutput { get; set; }

    public string StandardError { get; set; }
}