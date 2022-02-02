namespace ContinuousPrimate;

public static class NameSearch
{
    public static WordType? TryGetWordType(this WordDict wordDict, string text)
    {
        var key = AnagramKey.CreateCareful(text);
        foreach (var (wordType, lookup) in wordDict.Tables)
        {
            if (lookup.Lookup[key].Select(x=>x.Text).Contains(text, StringComparer.OrdinalIgnoreCase))
            {
                return wordType;
            }
        }

        return null;
    }


    public static (WordType wordType, bool comesFirst, SearchNode searchNode) GetSearchSettings(this WordType wordType)
    {
        return wordType switch
        {
            WordType.Noun => (WordType.Adjective, true, SearchNode.Name),
            WordType.Adjective => (WordType.Noun, false, SearchNode.Name),
            WordType.Verb => (WordType.Adverb, false, SearchNode.Name),
            WordType.Adverb => (WordType.Verb, true, SearchNode.Name),
            WordType.Other => (WordType.Noun, false, SearchNode.Name), //Not sure what to do here
            WordType.FirstName => (WordType.LastName, false, SearchNode.Default),
            WordType.LastName => (WordType.FirstName, true, SearchNode.Default),
            _ => throw new ArgumentOutOfRangeException(nameof(wordType), wordType, null)
        };
    }

    
    public static IEnumerable<PartialAnagram> Search(string name,
        WordDict wordDictionary)
    {
        if(string.IsNullOrWhiteSpace(name))
            return ArraySegment<PartialAnagram>.Empty;

        if (name.Contains(' ') || name.Length >= 15)
        {
            return FindAnagrams(name, wordDictionary, SearchNode.Default);
        }

        var wordType = TryGetWordType(wordDictionary, name)?? WordType.LastName;
        var (otherWordType, otherWordComesFirst, searchNode) = GetSearchSettings(wordType);

        return FindTwoWordAnagrams(name, otherWordType, otherWordComesFirst, wordDictionary, searchNode);
    }
    

    public static IEnumerable<PartialAnagram> FindAnagrams(
        string fullTerm,
        WordDict wordDictionary, SearchNode searchNode)
    {
        var key = AnagramKey.CreateCareful(fullTerm);
        var originalTerms = fullTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var anagrams = searchNode.FindAnagrams(originalTerms, key, ImmutableList<Word>.Empty, wordDictionary);

        foreach (var partialAnagram in anagrams)
        {
            yield return partialAnagram;
        }
    }


    public static IEnumerable<PartialAnagram> FindTwoWordAnagrams(
        string mainWord,
        WordType secondWordType,
        bool secondWordGoesFirst,
        WordDict wordDictionary, SearchNode searchNode)
    {
        var mainKey = AnagramKey.CreateCareful(mainWord);
        var originalTerms = ImmutableList<string>.Empty.Add(mainWord);


        foreach (var (secondWordKey, secondWord) in wordDictionary.Tables[secondWordType].List)
        {
            var combo = mainKey .Add(secondWordKey);
            var terms =
                secondWordGoesFirst?
                    originalTerms.Insert(0, secondWord.Text) :
                    originalTerms.Add(secondWord.Text);

            var anagrams = searchNode.FindAnagrams(terms,
                combo, ImmutableList<Word>.Empty, 
                wordDictionary).Take(1);

            foreach (var partialAnagram in anagrams)
                yield return partialAnagram;
            
        }
    }
}