namespace CommandSurfacer.Services;

public interface ICommandRunner
{
    public object Run(string[] args, params object[] additionalParameters) => Run<object>(string.Join(' ', args), additionalParameters);
    public T Run<T>(string[] args, params object[] additionalParameters) => Run<T>(string.Join(' ', args), additionalParameters);
    public object Run(string input, params object[] additionalParameters) => Run<object>(input, additionalParameters);
    public T Run<T>(string input, params object[] additionalParameters);

    public async Task<object> RunAsync(string[] args, params object[] additionalParameters) => await RunAsync<object>(string.Join(' ', args), additionalParameters);
    public async Task<T> RunAsync<T>(string[] args, params object[] additionalParameters) => await RunAsync<T>(string.Join(' ', args), additionalParameters);
    public async Task<object> RunAsync(string input, params object[] additionalParameters) => await RunAsync<object>(input, additionalParameters);
    public Task<T> RunAsync<T>(string input, params object[] additionalParameters);
}
