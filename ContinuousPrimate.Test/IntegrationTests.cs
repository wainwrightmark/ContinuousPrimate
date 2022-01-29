using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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


    private static Lazy<IEnumerable<(AnagramKey key, string name)>> FirstNames =
        WordListHelper.MakeEnumerable(Words.Names);

    private static Lazy<ILookup<AnagramKey, Word>> NounLookup =
        WordListHelper.MakeLookup(Words.Nouns);

    private static Lazy<ILookup<AnagramKey, Word>> AdjectiveLookup =
        WordListHelper.MakeLookup(Words.Adjectives);

    



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
            TestOutputHelper.WriteLine(partialAnagram.ToString());
        }
    }

    [Theory]
    [InlineData(PartOfSpeech.Adjective)]
    [InlineData(PartOfSpeech.Noun)]
    public void TestPartOfSpeech(PartOfSpeech partOfSpeech)
    {
        var wordNetEngine = new WordNetEngine();

        var firstNames = FirstNames.Value.Select(x => x.name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var words = wordNetEngine.GetAllSynSets()
            .Where(x => x.PartOfSpeech == partOfSpeech)
            
            .SelectMany(synSet => synSet.Words.Where(IsGoodWord).Select(word=> (word, synSet)))
            .GroupBy(x=>x.word, x=>x.synSet)
            .Distinct().ToList();

        TestOutputHelper.WriteLine(words.Count + " Words");

        foreach (var word in words)
        {
            var key = AnagramKey.Create(word.Key);
            var gloss = FormatGloss(word.First());

            TestOutputHelper.WriteLine($"{key.Text}\t{word.Key}\t{gloss}");
        }

        bool IsGoodWord(string word)
        {
            if (word.All(c => char.IsLetter(c) && char.IsLower(c)))
            {
                if (!firstNames.Contains(word))
                    return true;
            }

            return false;
        }
    }

    public static string FormatGloss(SynSet synSet)
    {
        if (string.IsNullOrWhiteSpace(synSet.Gloss)) return Format(synSet.Words.First());

        return Format(synSet.Gloss);


        static string Format(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim()
                .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];

            s = BracketRegex.Replace(s, "");

            if (char.IsLetter(s.First()))
                return s.Substring(0, 1).ToUpperInvariant() + s.Substring(1);

            return s;
        }
    }

    private static readonly Regex BracketRegex = new Regex("^\\s*\\(.*?\\)\\s*", RegexOptions.Compiled);

    [Theory]
    [InlineData("birthwort")]
    [InlineData("stitchwort")]
    [InlineData("twayblade")]
    [InlineData("whiptail")]
    [InlineData("lizard")]
    [InlineData("nightwalker")]
    [InlineData("wingman")]
    [InlineData("fuck")]
    [InlineData("dragon")]
    [InlineData("nervy")]
    public void TestWord(string word)
    {
        var wordNetEngine = new WordNetEngine();
        var synsets = wordNetEngine.GetSynSets(word).ToList();

        TestOutputHelper.WriteLine(word);
        TestOutputHelper.WriteLine(synsets.Count() + " Synsets");

        foreach (var synSet in synsets)   
        {
            TestOutputHelper.WriteLine($"{synSet.Id}: {synSet.Gloss}");
            TestOutputHelper.WriteLine(string.Join(", ", synSet.Words));

            TestOutputHelper.WriteLine(string.Join(", ", synSet.LexicalRelations.Select(x=>x)));
            TestOutputHelper.WriteLine(string.Join(", ", synSet.SemanticRelations.Select(x=>x)));
        }
    }
}