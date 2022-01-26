using System.Linq;
using Xunit;
using Xunit.Abstractions;
//using WordNet;

namespace ContinuousPrimate.Test;
public class UnitTest1
{
    public UnitTest1(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }

    

    public ITestOutputHelper TestOutputHelper { get; }

    [Theory]
    [InlineData("wainwright")]
    [InlineData("mark")]
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
        NameSearch.GetPartialAnagrams(mainName
        ).ToList();

        foreach (var partialAnagram in results)
        {
            var text = $"{partialAnagram} = {partialAnagram.Anagram}";

            TestOutputHelper.WriteLine(text);
        }
    }

    //[Theory]
    //[InlineData(PartOfSpeech.Adjective)]
    //[InlineData(PartOfSpeech.Noun)]
    //public void TestPartOfSpeech(PartOfSpeech partOfSpeech)
    //{
    //    var wordNetEngine = new WordNetEngine();

    //    var words = wordNetEngine.GetAllSynSets()
    //        .Where(x => x.PartOfSpeech == partOfSpeech)
    //        .SelectMany(x=>x.Words)
    //        .Where(x=> x.All(c=> char.IsLetter(c) && char.IsLower(c) ))
    //        .Distinct().ToList();

    //    TestOutputHelper.WriteLine(words.Count + " Words");

    //    foreach (var adjective in words)
    //    {
    //        TestOutputHelper.WriteLine(adjective);
    //    }
    //}
}