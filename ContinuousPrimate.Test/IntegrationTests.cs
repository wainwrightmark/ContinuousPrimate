using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq.Experimental;
using WordNet;
using Xunit;
using Xunit.Abstractions;
//using WordNet;

namespace ContinuousPrimate.Test;

public class IntegrationTests
{
    public IntegrationTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }    

    public ITestOutputHelper TestOutputHelper { get; }


    private static Lazy<IEnumerable<(AnagramKey key, string name)>> FirstNames= new(

        Words.Names
            .Split('\n', StringSplitOptions.TrimEntries)
            .Select(name=> (AnagramKey.Create(name), name)).Memoize()
            
    );
    private static Lazy<ILookup<AnagramKey, string>> NounLookup = new(()=>
        Words.Nouns
            .Split('\n', StringSplitOptions.TrimEntries)
            .ToLookup(AnagramKey.Create)
    );
    private static Lazy<ILookup<AnagramKey, string>> AdjectiveLookup = new(()=>
        Words.Adjectives
            .Split('\n', StringSplitOptions.TrimEntries)
            .ToLookup(AnagramKey.Create)
    );
    
        



    [Theory]
    [InlineData("beckham")]
    [InlineData("wainwright")]
    [InlineData("cheung")]
    [InlineData("scaysbrooke")]
    [InlineData("walker")]
    [InlineData("taylor")]
    [InlineData("johnson")]
    [InlineData("steverink")]
    [InlineData("curran")]
    [InlineData("onipko")]
    [InlineData("gumbel")]
    [InlineData("mote")]
    [InlineData("loake")]
    [InlineData("badenas")]
    [InlineData("thomas")]
    public void TestSearch(string mainName)
    {

        var results =
        NameSearch.FindNameAnagrams(mainName,
                FirstNames.Value,
                NounLookup.Value,
                AdjectiveLookup.Value
        );

        foreach (var partialAnagram in results)
        {
            var text = $"{partialAnagram.FullName} = {partialAnagram.Anagram}";

            TestOutputHelper.WriteLine(text);
        }
    }

    [Theory]
    [InlineData(PartOfSpeech.Adjective)]
    [InlineData(PartOfSpeech.Noun)]
    public void TestPartOfSpeech(PartOfSpeech partOfSpeech)
    {
        var wordNetEngine = new WordNetEngine();

        var words = wordNetEngine.GetAllSynSets()
            .Where(x => x.PartOfSpeech == partOfSpeech)
            .SelectMany(x => x.Words)
            .Where(x => x.All(c => char.IsLetter(c) && char.IsLower(c)))
            .Except(FirstNames.Value.Select(x=>x.name))
            .Distinct().ToList();

        TestOutputHelper.WriteLine(words.Count + " Words");

        foreach (var adjective in words)
        {
            TestOutputHelper.WriteLine(adjective);
        }
    }
}