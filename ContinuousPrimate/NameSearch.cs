namespace ContinuousPrimate;

public static class NameSearch
{
    public static (WordType wordType, bool comesFirst, SearchNode searchNode) GetSearchSettings(this WordType wordType)
    {
        return wordType switch
        {
            WordType.Noun => (WordType.Adjective, true, SearchNode.Name),
            WordType.Adjective => (WordType.Noun, false, SearchNode.Name),
            WordType.Verb => (WordType.Adverb, false, SearchNode.Name),
            WordType.Adverb => (WordType.Verb, true, SearchNode.Name),
            WordType.Other => (WordType.FirstName, false, SearchNode.Default), //Assume this was a last name
            WordType.FirstName => (WordType.LastName, false, SearchNode.Default),
            WordType.LastName => (WordType.FirstName, true, SearchNode.Default),
            _ => throw new ArgumentOutOfRangeException(nameof(wordType), wordType, null)
        };
    }

    
    public static IEnumerable<PartialAnagram> Search(string text,
        WordDict wordDictionary)
    {
        var words = GetWords(text, wordDictionary);
        if(words.Count == 0)return ArraySegment<PartialAnagram>.Empty;

        if (words.Count == 1)
        {
            var (otherWordType, otherWordComesFirst, searchNode) = GetSearchSettings(words.Single().WordType);
            return FindTwoWordAnagrams(words.Single(), otherWordType, otherWordComesFirst, wordDictionary, searchNode);
        }

        return FindAnagrams(words, wordDictionary, SearchNode.Default);
    }

    public static IReadOnlyList<Word> GetWords(string text, WordDict wordDict)
    {
        var terms = text.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var words = terms.Select(x => GetWord(wordDict, x)).ToList();

        return words;

        static Word GetWord(WordDict wordDict, string text)
        {
            var key = AnagramKey.CreateCareful(text);
            foreach (var (wordType, lookup) in wordDict.Tables)
            {
                var match = lookup.Lookup[key]
                    .FirstOrDefault(x => x.Text.Equals(text, StringComparison.OrdinalIgnoreCase));

                if (match is not null) return match;
            }

            return new Word(text, "", WordType.Other);
        }

    }

    public static IEnumerable<PartialAnagram> FindAnagrams(
        IReadOnlyList<Word> words,
        WordDict wordDictionary, SearchNode searchNode)
    {
        var key = AnagramKey.CreateCareful(string.Join("", words.Select(x=>x.Text)));
        

        var anagrams = searchNode.FindAnagrams(words, key, ImmutableList<Word>.Empty, wordDictionary);

        foreach (var partialAnagram in anagrams)
        {
            yield return partialAnagram;
        }
    }


    public static IEnumerable<PartialAnagram> FindTwoWordAnagrams(
        Word mainWord,
        WordType secondWordType,
        bool secondWordGoesFirst,
        WordDict wordDictionary, SearchNode searchNode)
    {
        var mainKey = AnagramKey.CreateCareful(mainWord.Text);
        var originalTerms = ImmutableList<Word>.Empty.Add(mainWord);


        foreach (var (secondWordKey, secondWord) in wordDictionary.Tables[secondWordType].List)
        {
            var combo = mainKey .Add(secondWordKey);
            var terms =
                secondWordGoesFirst?
                    originalTerms.Insert(0, secondWord) :
                    originalTerms.Add(secondWord);

            var anagrams = searchNode.FindAnagrams(terms,
                combo, ImmutableList<Word>.Empty, 
                wordDictionary).Take(1);

            foreach (var partialAnagram in anagrams)
                yield return partialAnagram;
            
        }
    }
}