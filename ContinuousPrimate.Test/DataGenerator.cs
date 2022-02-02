using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using WordNet;
using Xunit;
using Xunit.Abstractions;

namespace ContinuousPrimate.Test;

public class DataGenerator
{
    public DataGenerator(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    public ITestOutputHelper TestOutputHelper { get; }


    public static string GetFullWordDictText()
    {
        return string.Join("\n", GetFullWordDictLines());
    }

    private static readonly Lazy<IEnumerable<(AnagramKey key, string name)>> FirstNames = WordDictHelper.CreateEnumerable(Words.FirstNames);
    private static readonly Lazy<IEnumerable<(AnagramKey key, string name)>> LastNames = WordDictHelper.CreateEnumerable(Words.LastNames);

    public static IEnumerable<string> GetFullWordDictLines()
    {
        var wordNetEngine = new WordNetEngine();

        var firstNames = FirstNames.Value.Select(x => x.name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var lastNames = LastNames.Value.Select(x => x.name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var words = wordNetEngine.GetAllSynSets()
            //.Where(x => x.PartOfSpeech == partOfSpeech)
            .SelectMany(synSet => synSet.Words.Where(IsGoodWord).Select(word => (word, synSet)))
            .GroupBy(x => (x.word, x.synSet.PartOfSpeech), x => x.synSet)
            .Distinct().ToList();


        foreach (var grouping in words.OrderBy(x=>x.Key.PartOfSpeech))
        {
            var key = AnagramKey.Create(grouping.Key.word);
            var gloss = FormatGloss(grouping);

            if (gloss is not null)
            {
                yield return $"{GetAbbreviation(grouping.Key.PartOfSpeech)}\t{grouping.Key.word}\t{key.Text}\t{gloss}";
            }
        }

        foreach (var lastName in lastNames)
        {
            var key = AnagramKey.CreateCareful(lastName);
            yield return $"l\t{lastName}\t{key.Text}\t";
        }

        foreach (var firstName in firstNames)
        {
            var key = AnagramKey.CreateCareful(firstName);
            yield return $"f\t{firstName}\t{key.Text}\t";
        }

        static string GetAbbreviation(PartOfSpeech pos)
        {
            return pos switch
            {
                PartOfSpeech.None => "o",
                PartOfSpeech.Noun => "n",
                PartOfSpeech.Verb => "v",
                PartOfSpeech.Adjective => "j",
                PartOfSpeech.Adverb => "a",
                _ => throw new ArgumentOutOfRangeException(nameof(pos), pos, null)
            };
        }

        bool IsGoodWord(string word)
        {
            if (word.Length > 2)
            {
                if (word.All(c => char.IsLetter(c) && char.IsLower(c)))
                {
                    if (!firstNames.Contains(word) && !lastNames.Contains(word))
                        return true;
                }
            }

            return false;
        }
    }

    [Fact]
    public void CreateFullWordDictFile()
    {
        var lines = GetFullWordDictLines().ToList();

        TestOutputHelper.WriteLine($"{lines.Count} Words");
        TestOutputHelper.WriteLine("Writing File");

        using var file =
            File.OpenWrite(
                @"C:\Users\wainw\source\repos\MarkPersonal\ContinuousPrimate1\ContinuousPrimate.Blazor\wwwroot\Data\WordData.gzip");
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

    public static string? FormatGloss(IGrouping<(string, PartOfSpeech), SynSet> grouping)
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
}