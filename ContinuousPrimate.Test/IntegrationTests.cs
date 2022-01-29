using System;
using System.Linq;
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
        throw new NotImplementedException();
        //var results = 
        //NameSearch.GetPartialAnagrams(mainName
        //);

        //foreach (var partialAnagram in results)
        //{
        //    var text = $"{partialAnagram.FullName} = {partialAnagram.Anagram}";

        //    TestOutputHelper.WriteLine(text);
        //}
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