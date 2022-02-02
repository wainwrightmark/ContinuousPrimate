using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using MoreLinq.Experimental;
using WordNet;
using Xunit;
using Xunit.Abstractions;
using WordDict = System.Collections.Generic.IReadOnlyDictionary<ContinuousPrimate.PartOfSpeech, System.Collections.Generic.IReadOnlyDictionary<ContinuousPrimate.AnagramKey, ContinuousPrimate.Word>>;
//using WordNet;

namespace ContinuousPrimate.Test;

public class IntegrationTests
{
    public IntegrationTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; }


    private static readonly Lazy<IEnumerable<(AnagramKey key, string name)>> FirstNames =
        WordListHelper.CreateEnumerable(Words.Names);

    private static readonly Lazy<WordDict> FullWordDict =
        new(()=>
            WordListHelper.CreateFullWordDictionary(GetFullWordDictText())
            );

    [Theory]
    [InlineData("Wainwright", "wily nightWalker") ]
    [InlineData("Angela Curran", "A nuclear gran") ]
    [InlineData("Stephanie Cheung", "An ensuing peach") ]

    public void TestResultsContains(string fulltext, string expected)
    {
        var results =
            NameSearch.Search(fulltext,
                FirstNames.Value,
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
    public void TestSearch(string mainName)
    {

        var results =
        NameSearch.FindNameAnagrams(mainName,
                FirstNames.Value,
                FullWordDict.Value
        ).Memoize();

        results.Take(100).Count().Should().Be(100);

        foreach (var partialAnagram in results.Take(100))
        {
            TestOutputHelper.WriteLine(partialAnagram.ToString());
            CheckValidity(partialAnagram);
        }
        //TestOutputHelper.WriteLine($"{results.Count()} Names Found");
    }

    private static void CheckValidity(PartialAnagram pa)
    {
        var terms = AnagramKey.CreateCareful(pa.TermsText);
        var anagram = AnagramKey.CreateCareful(pa.AnagramText);

        terms.Should().Be(anagram);
    }

    public static string GetFullWordDictText()
    {
        return string.Join("\n", GetFullWordDictLines().OrderBy(x => x));
    }
    public static IEnumerable<string> GetFullWordDictLines()
    {
        var wordNetEngine = new WordNetEngine();

        var firstNames = FirstNames.Value.Select(x => x.name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var groupings = wordNetEngine.GetAllSynSets()
            //.Where(x => x.PartOfSpeech == partOfSpeech)
            .SelectMany(synSet => synSet.Words.Where(IsGoodWord).Select(word => (word, synSet)))
            .GroupBy(x => (x.word, x.synSet.PartOfSpeech), x => x.synSet)
            .Distinct().ToList();

        

        foreach (var grouping in groupings)
        {
            var key = AnagramKey.Create(grouping.Key.word);
            var gloss = FormatGloss(grouping);

            if (gloss is not null)
            {
                yield return $"{GetAbbreviation(grouping.Key.PartOfSpeech)}\t{grouping.Key.word}\t{key.Text}\t{gloss}";
            }
        }

        
        static string GetAbbreviation(WordNet.PartOfSpeech pos)
        {
            return pos switch
            {
                WordNet.PartOfSpeech.None => "o",
                WordNet.PartOfSpeech.Noun => "n",
                WordNet.PartOfSpeech.Verb => "v",
                WordNet.PartOfSpeech.Adjective => "j",
                WordNet.PartOfSpeech.Adverb => "a",
                _ => throw new ArgumentOutOfRangeException(nameof(pos), pos, null)
            };
        }

        bool IsGoodWord(string word)
        {
            if (word.Length > 2)
            {
                if (word.All(c => char.IsLetter(c) && char.IsLower(c)))
                {
                    if (!firstNames.Contains(word))
                        return true;
                }
            }
            return false;
        }
    }

    [Fact]
    public void CreateFullWordDictFile()
    {
        var lines = GetFullWordDictLines().OrderBy(x => x).ToList();


        TestOutputHelper.WriteLine($"{lines.Count} Words");
        TestOutputHelper.WriteLine("Writing File");

        using var file = File.OpenWrite(@"C:\Users\wainw\source\repos\MarkPersonal\ContinuousPrimate1\ContinuousPrimate.Blazor\wwwroot\Data\WordData.gzip");
        using var gZipStream = new GZipStream(file, CompressionMode.Compress);
        using (var writer = new StreamWriter(gZipStream))
        {
            foreach (var line in lines)
            {
                writer.Write(line);
                writer.Write('\n');
            }

        }

        file.Close();
        TestOutputHelper.WriteLine("File Written");

        
    }

    public static string? FormatGloss(IGrouping<(string, WordNet.PartOfSpeech), SynSet> grouping)
    {
        var gloss = grouping
            .Select(x => Format(x.Gloss))
            .OrderBy(x => x.Contains(grouping.Key.Item1)) //Avoid glosses which contain the word
            .ThenBy(x => x.Length)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(gloss))
        {
            return null;
        }

        return gloss;


        static string Format(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            s = s.Trim()
                .Split(new[] { ';', '|' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)[0];

            s = BracketRegex.Replace(s, "");

            if (string.IsNullOrWhiteSpace(s)) return s;

            if (char.IsLetter(s.First()))
                return s[..1].ToUpperInvariant() + s[1..];

            return s;
        }
    }

    private static readonly Regex BracketRegex = new("^\\s*\\(.*?\\)\\s*", RegexOptions.Compiled);

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