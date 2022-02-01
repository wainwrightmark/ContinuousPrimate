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
        WordDict wordDictionary
    )
    {
        var key = AnagramKey.CreateCareful(fullTerm);
        var originalTerms = fullTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var adjectives = wordDictionary[PartOfSpeech.Adjective];
        var nouns = wordDictionary[PartOfSpeech.Noun];


        foreach (var (adjectiveKey, adjective) in adjectives)
        {
            var remainder = key.TrySubtract(adjectiveKey);
            if (remainder .HasValue)
            {
                if (nouns.TryGetValue(remainder.Value, out var noun))
                {
                    yield return new PartialAnagram(originalTerms,
                        new List<Word>(){adjective, noun}
                    );
                }
            }
        }
    }


    public static IEnumerable<PartialAnagram> FindNameAnagrams(
        string mainWord,
        IEnumerable<(AnagramKey key, string word)> names,
        WordDict wordDictionary)
    {
        var mainKey = AnagramKey.CreateCareful(mainWord);
        var adjectives = wordDictionary[PartOfSpeech.Adjective];
        var nouns = wordDictionary[PartOfSpeech.Noun];


        foreach (var (key, firstName) in names)
        {
            var combo = mainKey.Add(key);
            foreach (var  (adjectiveKey, adjective)  in adjectives)
            {
                var remainder = combo.TrySubtract(adjectiveKey);
                if (remainder .HasValue)
                {
                    if(adjectiveKey == mainKey)continue; //Best to do this check here - we get an extra subtraction but skip a bunch of checks
                    if(remainder.Value == mainKey)continue;

                    if (nouns.TryGetValue(remainder.Value, out var noun))
                    {
                        yield return new PartialAnagram(
                            new List<string>(){firstName, mainWord},
                            new List<Word>(){adjective, noun}
                        );
                        break; //now try a new name - it's dumb to have more than one anagram per name
                    }
                }
            }
            
        }
    }
}