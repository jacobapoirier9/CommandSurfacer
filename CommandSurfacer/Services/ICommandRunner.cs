namespace CommandSurfacer.Services;

public interface ICommandRunner
{
    public object Run(string[] args) => Run<object>(string.Join(' ', args));
    public T Run<T>(string[] args) => Run<T>(string.Join(' ', args));
    public object Run(string input) => Run<object>(input);
    public T Run<T>(string input);

    public async Task<object> RunAsync(string[] args) => await RunAsync<object>(string.Join(' ', args));
    public async Task<T> RunAsync<T>(string[] args) => await RunAsync<T>(string.Join(' ', args));
    public async Task<object> RunAsync(string input) => await RunAsync<object>(input);
    public Task<T> RunAsync<T>(string input);
}
