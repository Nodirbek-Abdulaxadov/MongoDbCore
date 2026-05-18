namespace MongoDbCore;

/// <summary>
/// Pluralization + underscore casing used to derive Mongo collection names
/// from CLR type names (e.g. <c>TodoItem</c> -> <c>todo_items</c>).
/// </summary>
internal static class Inflector
{
    private static readonly List<Rule> _plurals = [];
    private static readonly HashSet<string> _uncountables = [];

    static Inflector()
    {
        AddPlural("$", "s");
        AddPlural("s$", "s");
        AddPlural("(ax|test)is$", "$1es");
        AddPlural("(octop|vir|alumn|fung)us$", "$1i");
        AddPlural("(alias|status)$", "$1es");
        AddPlural("(bu)s$", "$1ses");
        AddPlural("(buffal|tomat|volcan)o$", "$1oes");
        AddPlural("([ti])um$", "$1a");
        AddPlural("sis$", "ses");
        AddPlural("(?:([^f])fe|([lr])f)$", "$1$2ves");
        AddPlural("(hive)$", "$1s");
        AddPlural("([^aeiouy]|qu)y$", "$1ies");
        AddPlural("(x|ch|ss|sh)$", "$1es");
        AddPlural("(matr|vert|ind)ix|ex$", "$1ices");
        AddPlural("([m|l])ouse$", "$1ice");
        AddPlural("^(ox)$", "$1en");
        AddPlural("(quiz)$", "$1zes");

        AddIrregular("person", "people");
        AddIrregular("man", "men");
        AddIrregular("child", "children");
        AddIrregular("sex", "sexes");
        AddIrregular("move", "moves");
        AddIrregular("goose", "geese");
        AddIrregular("alumna", "alumnae");

        foreach (var w in new[] { "equipment", "information", "rice", "money", "species", "series", "fish", "sheep", "deer", "aircraft" })
            _uncountables.Add(w);
    }

    private sealed class Rule(string pattern, string replacement)
    {
        private readonly Regex _regex = new(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly string _replacement = replacement;

        public string? Apply(string word) => _regex.IsMatch(word) ? _regex.Replace(word, _replacement) : null;
    }

    private static void AddIrregular(string singular, string plural)
        => AddPlural("(" + singular[0] + ")" + singular[1..] + "$", "$1" + plural[1..]);

    private static void AddPlural(string rule, string replacement)
        => _plurals.Add(new Rule(rule, replacement));

    internal static string Pluralize(this string word)
    {
        if (_uncountables.Contains(word.ToLowerInvariant())) return word;
        for (int i = _plurals.Count - 1; i >= 0; i--)
        {
            var result = _plurals[i].Apply(word);
            if (result is not null) return result;
        }
        return word;
    }

    internal static string Underscore(this string pascalCasedWord)
        => Regex.Replace(
               Regex.Replace(
                   Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"),
                   @"([a-z\d])([A-Z])", "$1_$2"),
               @"[-\s]", "_").ToLowerInvariant();
}
