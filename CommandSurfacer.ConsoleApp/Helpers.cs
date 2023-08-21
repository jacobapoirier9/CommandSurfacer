namespace CommandSurfacer.ConsoleApp;

public static class Helpers
{
    private class IndexedItem<TItem>
    {
        public int Index { get; set; }

        public TItem Item { get; set; }
    }

    private static IEnumerable<IndexedItem<TItem>> AsIndexItems<TItem>(this IEnumerable<TItem> items)
    {
        var index = 0;
        var result = items.Select(item => new IndexedItem<TItem>
        {
            Index = index++,
            Item = item
        });

        return result;
    }

    public static IEnumerable<TOut> JoinOnIndex<TLeft, TRight, TOut>(IEnumerable<TLeft> leftEnumerable, IEnumerable<TRight> rightEnumerable, Func<TLeft, TRight, TOut> select)
    {
        if (leftEnumerable.Count() != rightEnumerable.Count())
            throw new ApplicationException($"{nameof(leftEnumerable)} has {leftEnumerable.Count()} items, where {nameof(rightEnumerable)} has {rightEnumerable.Count()} items.");

        var leftIndexedItems = AsIndexItems(leftEnumerable);
        var rightIndexedItems = AsIndexItems(rightEnumerable);

        var joinedItems = (
            from left in leftIndexedItems
            join right in rightIndexedItems
                on left.Index equals right.Index
            select
                @select(left.Item, right.Item)
        );

        return joinedItems;
    }
}

//public class Xml
//{
//    private readonly XmlDocument _document;

//    public Xml(string xml)
//    {
//        Console.WriteLine(xml);

//        _document = new XmlDocument();
//        _document.LoadXml(xml);
//    }



//    public T ParseXPath<T>(string xpath)
//    {
//        var navigator = _document.CreateNavigator();
//        var nodes = navigator.Select("//name");

//        foreach (XPathNavigator node in nodes)
//        {
//            var name = node.Name;
//            var value = node.Value;

//            var v = node.ValueAs(typeof(string));
//            Console.WriteLine(v);
//        }

//        return default;
//    }
//}