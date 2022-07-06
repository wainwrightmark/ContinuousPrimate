using System.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ContinuousPrimate.Test;

public class AnagramKeyTests
{
    public AnagramKeyTests(ITestOutputHelper testOutputHelper)
    {
        TestOutputHelper = testOutputHelper;
    }    

    public ITestOutputHelper TestOutputHelper { get; }

    [Theory]
    [InlineData("white wash", "aehhistww")]
    [InlineData("red blue", "bdeelru")]
    public void TestAdd(string keysToAdd, string expected)
    {
        var actual = keysToAdd.Split(" ")
            .Select(AnagramKey.Create)
            .Aggregate((a,b)=>a.Add(b));

        actual.GetText().Should().Be(expected);
    }

    [Theory]
    [InlineData("michaelbeckham", "semimonthly", null)]
    [InlineData("kellywainwright", "nightwalker", "ilwy")]
    public void TestSubtract(string keyText, string subtractText, string? expected)
    {
        var key1 = AnagramKey.Create(keyText);
        var key2 = AnagramKey.Create(subtractText);

        var actual = key1.TrySubtract(key2);

        actual?.GetText().Should().Be(expected);
    }
}