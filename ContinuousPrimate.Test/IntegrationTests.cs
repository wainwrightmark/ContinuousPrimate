using System;
using System.Linq;
using FluentAssertions;
using MoreLinq.Experimental;
using WordNet;
using Xunit;
using Xunit.Abstractions;

namespace ContinuousPrimate.Test;

public class IntegrationTests
{
    public IntegrationTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; }

    private static readonly Lazy<WordDict> FullWordDict =
        new(()=> WordDict.Create(DataGenerator.GetFullWordDictText())
            );

    [Theory]
    [InlineData("Wainwright", "wily nightwalker") ]
    [InlineData("Angela Curran", "A nuclear gran") ]
    [InlineData("Stephanie Cheung", "The ensuing peach") ]

    public void TestResultsContains(string fulltext, string expected)
    {
        var results =
            NameSearch.Search(fulltext,
                FullWordDict.Value
            ).Take(100);

        results.Select(x => x.AnagramText).Should().Contain(expected);
    }

    [Theory]
    [InlineData("Beckham")]
    [InlineData("Wainwright")]
    [InlineData("Cheung")]
    [InlineData("Scaysbrooke")]
    [InlineData("Walker")]
    [InlineData("Taylor")]
    [InlineData("Johnson")]
    [InlineData("Steverink")]
    [InlineData("Curran")]
    [InlineData("Onipko")]
    [InlineData("Gumbel")]
    [InlineData("Mote")]
    [InlineData("Loake")]
    [InlineData("Badenas")]
    [InlineData("Thomas")]
    [InlineData("Dragon")]
    [InlineData("Laser")]
    public void TestSearch(string mainName)
    {
        var results =
        NameSearch.Search(mainName,
            FullWordDict.Value
        ).Memoize();

        results.Take(100).Count().Should().Be(100);

        foreach (var partialAnagram in results.Take(100))
        {
            TestOutputHelper.WriteLine(partialAnagram.ToString());
            CheckValidity(partialAnagram);
        }
    }

    private static void CheckValidity(PartialAnagram pa)
    {
        var terms = AnagramKey.CreateCareful(pa.TermsText);
        var anagram = AnagramKey.CreateCareful(pa.AnagramText);

        terms.Should().Be(anagram);
    }



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
        TestOutputHelper.WriteLine(synsets.Count + " Synsets");

        foreach (var synSet in synsets)
        {
            TestOutputHelper.WriteLine($"{synSet.Id}: {synSet.Gloss}");
            TestOutputHelper.WriteLine(string.Join(", ", synSet.Words));

            TestOutputHelper.WriteLine(string.Join(", ", synSet.LexicalRelations.Select(x => x)));
            TestOutputHelper.WriteLine(string.Join(", ", synSet.SemanticRelations.Select(x => x)));
        }
    }
}