//using System.Text;

//namespace CommandSurfacer.Services;

//public class BetterArgsParser : IBetterArgsParser
//{
//    // https://stackoverflow.com/questions/59638467/parsing-command-line-args-with-quotes

//    private List<string> ParseCommandLine(string input)
//    {
//        var args = new List<string>();

//        if (string.IsNullOrWhiteSpace(input))
//            return args;

//        var builder = new StringBuilder();
//        var quotingCharacter = default(char?);

//        // Step 2
//        for (var i = 0; i < input.Length; i++)
//        {
//            var character = input[i];

//            var inQuotes = quotingCharacter.HasValue;

//            if (character == '"' || character == '\'')
//            {
//                if (inQuotes)
//                {
//                    args.Add(builder.ToString());
//                    builder.Clear();
//                    quotingCharacter = default(char?);
//                }
//                else
//                {
//                    quotingCharacter = character;
//                }
//            }
//            else if (character == ' ' || character == ',')
//            {
//                if (inQuotes)
//                {
//                    builder.Append(character);
//                }
//                else if (builder.Length > 0)
//                {
//                    args.Add(builder.ToString());
//                    builder.Clear();
//                }
//            }
//            else
//                builder.Append(character);
//        }

//        // Step 3
//        if (builder.Length > 0)
//            args.Add(builder.ToString());

//        return args;
//    }

//    public Arguments Parse(string input)
//    {
//        var args = ParseCommandLine(input);

//        var list = new List<string>();
//        var dictionary = new Dictionary<string, List<string>>();

//        var prefixes = new string[] { "--", "-", "/" };
//        var separators = new string[] { "=", ":" };
//        var separatedValueTrims = new char[] { '"', '\'', ',' };

//        var currentKey = default(string);
//        for (var i = 0; i < args.Count; i++)
//        {
//            var item = args[i];

//            if (Utils.StartsWithAny(item, prefixes, out var prefix))
//            {
//                currentKey = item.Replace(prefix, string.Empty);

//                if (Utils.ContainsAny(currentKey, separators, out var separator))
//                {
//                    var split = currentKey.Split(separator);

//                    currentKey = split[0];
//                    dictionary.Add(currentKey, new List<string>() { split[1].Trim(separatedValueTrims) });
//                }
//                else if (!dictionary.ContainsKey(currentKey))
//                    dictionary.Add(currentKey, new List<string>());
//            }
//            else if (currentKey is null)
//                list.Add(item);
//            else
//                dictionary[currentKey].Add(item);
//        }

//        return new Arguments
//        {
//            List = list,
//            Dictionary = dictionary
//        };
//    }
//}