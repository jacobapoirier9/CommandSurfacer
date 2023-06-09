﻿using CommandSurfacer.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ObjectiveC;

namespace CommandSurfacer.Services;

public class CommandRunner : ICommandRunner
{
    private readonly IArgsParser _argsParser;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISendHelpMessages _sendHelpMessages;

    public CommandRunner(IArgsParser argsParser, IServiceProvider serviceProvider)
    {
        _argsParser = argsParser;
        _serviceProvider = serviceProvider;

        _sendHelpMessages = serviceProvider.GetService<ISendHelpMessages>();
    }

    private object RunCommand(string input, params object[] additionalParameters)
    {
        var options = _argsParser.ParseTypedValue<CommonSurfaceOptions>(ref input);
        if (options.ProvidedHelpSwitch && _sendHelpMessages is not null)
        {
            var surface = _argsParser.ResolveSurfaceAttributeOrDefault(input);
            if (surface is null)
            {
                var group = _argsParser.ResolveGroupAttributeOrDefault(input);
                if (group is not null)
                    _sendHelpMessages.SendClientHelp(group);
                else
                    _sendHelpMessages.SendClientHelp();
            }
            else
                _sendHelpMessages.SendClientHelp(surface);

            return default;
        }

        additionalParameters = Utils.CombineArrays(additionalParameters, options);

        var target = _argsParser.ParseCommandSurface(ref input);

        var parameters = _argsParser.ParseMethodParameters(ref input, target.Method, additionalParameters);

        var instance = _serviceProvider.GetRequiredService(target.Type);
        var result = target.Method.Invoke(instance, parameters);

        return result;
    }

    public T Run<T>(string input, params object[] additionalParameters)
    {
        var result = RunCommand(input, additionalParameters);

        if (result is Task<T> typedTask)
        {
            return typedTask.GetAwaiter().GetResult();
        }
        else if (result is Task emptyTask)
        {
            emptyTask.Wait();
            return default;
        }
        else
        {
            return (T)(object)result;
        }
    }

    public async Task<T> RunAsync<T>(string input, params object[] additionalParameters)
    {
        var result = RunCommand(input, additionalParameters);

        if (result is Task<T> typedTask)
        {
            return await typedTask;
        }
        else if (result is Task emptyTask)
        {
            await emptyTask;
            return await Task.FromResult<T>(default);
        }
        else
        {
            return await Task.FromResult<T>((T)result);
        }
    }
}