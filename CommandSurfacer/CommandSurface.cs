﻿using System.Reflection;

namespace CommandSurfacer;

public class CommandSurface
{
    public Type Type { get; set; }

    public GroupAttribute Group { get; set; }

    public MethodInfo Method { get; set; }

    public SurfaceAttribute Surface { get; set; }
}
