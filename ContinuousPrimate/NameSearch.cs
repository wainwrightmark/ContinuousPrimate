using System.Collections.Immutable;
using WordDict = System.Collections.Generic.IReadOnlyDictionary<ContinuousPrimate.PartOfSpeech, System.Collections.Generic.IReadOnlyDictionary<ContinuousPrimate.AnagramKey, ContinuousPrimate.Word>>;

namespace ContinuousPrimate;

public static class NameSearch
{
    
    public static IEnumerable<PartialAnagram> Search(string name,
        IEnumerable<(AnagramKey key, string name)> firstNames,
        WordDict wordDictionary
        )
    {
        if(string.IsNullOrWhiteSpace(name))
            return ArraySegment<PartialAnagram>.Empty;

        if (name.Contains(' ') || name.Length >= 15)
        {
            return FindAnagrams(name, wordDictionary);
        }

        return FindNameAnagrams(name, firstNames, wordDictionary);
    }
    

    public static IEnumerable<PartialAnagram> FindAnagrams(
        string fullTerm,
        WordDict wordDictionary)
    {
        var key = AnagramKey.CreateCareful(fullTerm);
        var originalTerms = fullTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var anagrams = SearchNode.Default.FindAnagrams(originalTerms, key, ImmutableList<Word>.Empty, wordDictionary);

        foreach (var partialAnagram in anagrams)
        {
            yield return partialAnagram;
        }
    }


    public static IEnumerable<PartialAnagram> FindNameAnagrams(
        string mainWord,
        IEnumerable<(AnagramKey key, string word)> names,
        WordDict wordDictionary)
    {
        var mainKey = AnagramKey.CreateCareful(mainWord);
        var originalTerms = ImmutableList<string>.Empty.Add(mainWord);

        foreach (var (key, firstName) in names)
        {
            var combo = mainKey .Add(key);
            var terms = originalTerms.Insert(0, firstName);

            var anagrams = SearchNode.Default.FindAnagrams(terms,
                combo, ImmutableList<Word>.Empty, 
                wordDictionary).Take(1);

            foreach (var partialAnagram in anagrams)
                yield return partialAnagram;
            
        }
    }
}