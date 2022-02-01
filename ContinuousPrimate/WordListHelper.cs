using System.Diagnostics;
using MoreLinq.Experimental;

namespace ContinuousPrimate;

public static class WordListHelper
{
    public static Lazy<IEnumerable<(AnagramKey key, string name)>> CreateEnumerable(string data)
    {
        return new Lazy<IEnumerable<(AnagramKey key, string name)>>(()=>data
            .Split('\n', StringSplitOptions.TrimEntries)
            .Select(name => (AnagramKey.Create(name), name)).Memoize()
        ) ;
    }

    public static IReadOnlyDictionary<PartOfSpeech, IReadOnlyDictionary<AnagramKey, Word>> CreateFullWordDictionary(string data)
    {
        var sw = Stopwatch.StartNew();

        var dict = data.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(GetData)
            .GroupBy(x => x.partOfSpeech)
            .ToDictionary(x => x.Key
                , x =>
                    x.GroupBy(y=>y.anagramKey)
                        
                        
                        .ToDictionary(y => y.Key, 
                            y => y.First().word) as IReadOnlyDictionary<AnagramKey, Word>
            );

        Console.WriteLine(DateTime.Now + ": Word Dictionary Created " + sw.Elapsed);

        return dict;


        static (PartOfSpeech partOfSpeech, AnagramKey anagramKey, Word word) GetData(string line)
        {
            var terms = line.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var pos = GetPartOfSpeech(terms[0]);
            var word = terms[1];
            var key = new AnagramKey(terms[2]);
            var gloss = terms[3];

            return (pos, key, new Word(word, gloss));
        }
    }

    public static PartOfSpeech GetPartOfSpeech(string s)
    {
        if (s == "n") return PartOfSpeech.Noun;
        if (s == "j") return PartOfSpeech.Adjective;
        if (s == "a") return PartOfSpeech.Adverb;
        if (s == "v") return PartOfSpeech.Verb;
        return PartOfSpeech.Other;
    }
}