using System.Collections;

namespace CommandSurfacer;

public class Arguments : IEnumerable<string>
{
    public IEnumerable<string> Keys => Dictionary.Keys;
    public IEnumerable<string> Values => List.Concat(Dictionary.SelectMany(kvp => kvp.Value));


    public IList<string> List { get; set; }

    public IDictionary<string, List<string>> Dictionary { get; set; }


    IEnumerator IEnumerable.GetEnumerator() => Values.GetEnumerator();
    public IEnumerator<string> GetEnumerator() => Values.GetEnumerator();
}
