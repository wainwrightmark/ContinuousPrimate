namespace ContinuousPrimate;


public record WordDict(IReadOnlyDictionary<WordType, WordDict.Table> Tables)
{
    public record Table(IReadOnlyList<(AnagramKey Key, Word Word)> List, ILookup<AnagramKey, Word> Lookup);
}



public static class WordDictHelper
{
    public static Lazy<IEnumerable<(AnagramKey key, string name)>> CreateEnumerable(string data)
    {
        return new Lazy<IEnumerable<(AnagramKey key, string name)>>(()=>data
            .Split('\n', StringSplitOptions.TrimEntries)
            .Select(name => (AnagramKey.Create(name), name)).Memoize()
        ) ;
    }

    public static WordDict CreateFullWordDictionary(string data)
    {
        var sw = Stopwatch.StartNew();

        var tablesDict = new Dictionary<WordType, WordDict.Table>();

        var groups = data.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(GetData)
            .GroupBy(x => x.word.WordType);

        foreach (var group in groups)
        {
            var list = group.ToList();
            var lookup = group.ToLookup(x => x.anagramKey, x => x.word);

            var table = new WordDict.Table(list, lookup);

            tablesDict.Add(group.Key, table);
        }

        Console.WriteLine(DateTime.Now + ": Word Dictionary Created " + sw.Elapsed);
        return new WordDict(tablesDict);


        static (AnagramKey anagramKey, Word word) GetData(string line)
        {
            var terms = line.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var pos = GetPartOfSpeech(terms[0]);
            var word = terms[1];
            var key = new AnagramKey(terms[2]);
            var gloss = terms.Length > 3? terms[3] : "";

            return (key, new Word(word, gloss, pos));
        }
    }

    public static WordType GetPartOfSpeech(string s)
    {
        if (s == "n") return WordType.Noun;
        if (s == "j") return WordType.Adjective;
        if (s == "a") return WordType.Adverb;
        if (s == "v") return WordType.Verb;
        if (s == "f") return WordType.FirstName;
        if (s == "l") return WordType.LastName;
        return WordType.Other;
    }
}