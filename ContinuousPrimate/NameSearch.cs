using System.Text;
using MoreLinq.Experimental;
namespace ContinuousPrimate;

public record Word(string Text, string Gloss);


public static class WordListHelper
{
    public static Lazy<ILookup<AnagramKey, Word>> MakeLookup(string data)
    {
        return
            new Lazy<ILookup<AnagramKey, Word>>(() =>
                {
                    var result = data
                        .Split('\n', StringSplitOptions.TrimEntries)
                        .Select(Split)
                        .ToLookup(x => x.Key, x => x.Word);

                    Console.WriteLine(DateTime.Now + ": Lookup Created");

                    return result;
                }
                );

        static (AnagramKey Key, Word Word) Split(string s)
        {
            var terms = s.Split('\t');
            var key = new AnagramKey(terms[0]);
            var text = terms[1];
            var gloss = terms[2];

            return new ValueTuple<AnagramKey, Word>(key, new Word(text, gloss));
        }
    }

    public static Lazy<IEnumerable<(AnagramKey key, string name)>> MakeEnumerable(string data)
    {
        return new Lazy<IEnumerable<(AnagramKey key, string name)>>(()=>data
                .Split('\n', StringSplitOptions.TrimEntries)
                .Select(name => (AnagramKey.Create(name), name)).Memoize()
                ) ;
    }
}

public static class NameSearch
{
    public static IEnumerable<PartialAnagram> Search(string name,
        IEnumerable<(AnagramKey key, string name)> firstNames,
        ILookup<AnagramKey, Word> nounLookup,
        ILookup<AnagramKey, Word> adjectiveLookup
        )
    {
        if(string.IsNullOrWhiteSpace(name))
            return ArraySegment<PartialAnagram>.Empty;

        if (name.Contains(" ") || name.Length >= 15)
        {
            return FindAnagrams(name, nounLookup, adjectiveLookup);
        }

        return FindNameAnagrams(name, firstNames, nounLookup, adjectiveLookup);
    }
    

    public static IEnumerable<PartialAnagram> FindAnagrams(
        string fullTerm,
        ILookup<AnagramKey, Word> nouns,
        ILookup<AnagramKey, Word> adjectives
    )
    {
        var key = AnagramKey.CreateCareful(fullTerm);
        var originalTerms = fullTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var adjective in adjectives)
        {
            var remainder = key.TrySubtract(adjective.Key);
            if (remainder .HasValue)
            {

                var noun = nouns[remainder.Value].FirstOrDefault();

                if (noun is not null)
                {
                    yield return new PartialAnagram(originalTerms,
                        new List<Word>(){adjective.First(), noun}
                    );
                }
            }
        }
    }


    public static IEnumerable<PartialAnagram> FindNameAnagrams(
        string mainWord,
        IEnumerable<(AnagramKey key, string word)> names,
        ILookup<AnagramKey, Word> nouns,
        ILookup<AnagramKey, Word> adjectives)
    {
        var mainKey = AnagramKey.CreateCareful(mainWord);
        
        foreach (var (key, firstName) in names)
        {
            var combo = mainKey.Add(key);
            foreach (var adjective in adjectives)
            {
                var remainder = combo.TrySubtract(adjective.Key);
                if (remainder .HasValue)
                {
                    if(adjective.Key == mainKey)continue; //Best to do this check here - we get an extra subtraction but skip a bunch of checks
                    if(remainder.Value == mainKey)continue;

                    var noun = nouns[remainder.Value].FirstOrDefault();

                    if (noun is not null)
                    {
                        yield return new PartialAnagram(
                            new List<string>(){firstName, mainWord},
                            new List<Word>(){adjective.First(), noun}
                        );
                        break; //now try a new name - it's dumb to have more than one anagram per name
                    }
                }
            }
            
        }
    }
}

public readonly record struct AnagramKey(string Text)
{
    /// <summary>
    /// Create from a piece of text
    /// </summary>
    public static AnagramKey Create(string s)
        => new(new string(s.ToLowerInvariant().OrderBy(x => x).ToArray()));
    
    /// <summary>
    /// Create from a piece of text that might contain special characters
    /// </summary>
    public static AnagramKey CreateCareful(string s)
        => new(new string(s.Trim().ToLowerInvariant().Where(char.IsLetter).OrderBy(x => x).ToArray()));

    public AnagramKey Add(AnagramKey other)
    {
        int i = 0, j = 0;
        StringBuilder sb = new();

        while (i < Text.Length && j < other.Text.Length)
        {
            if (Text[i] < other.Text[j])
                sb.Append(Text[i++]);
            else
                sb.Append(other.Text[j++]);
        }

        if (i < Text.Length)
            sb.Append(Text[i..]);
        if (j < other.Text.Length)
            sb.Append(other.Text[j..]);

        return new AnagramKey(sb.ToString());
    }

    public AnagramKey? TrySubtract(AnagramKey other)
    {
        if (other.Text.Length > Text.Length)
            return null; //Impossible

        if (string.IsNullOrEmpty(other.Text))
            return this; //No change

        var sb = new StringBuilder();
        
        var otherIndex = 0;
        for (var thisIndex = 0; thisIndex < Text.Length; thisIndex++)
        {
            if (otherIndex >= other.Text.Length)
            {
                //We've finished consuming the other string
                sb.Append(Text[thisIndex..]);

                break;
            }
            
            var thisChar = Text[thisIndex];
            var otherChar = other.Text[otherIndex];

            if (thisChar == otherChar) //Same character in both - ignore it
            {
                otherIndex++;
            }
            else if(thisChar < otherChar)//Extra character in this
            {
                sb.Append(thisChar);
            }
            else
            {
                return null; //There is a char in other which isn't in this
            }
        }

        if (otherIndex < other.Text.Length) return null; //There are remaining characters in other we shouldn't return

        return new AnagramKey(sb.ToString());
    }
    
}

public record PartialAnagram(IReadOnlyList<string> OriginalTerms, IReadOnlyList<Word> AnagramWords)
{
    /// <inheritdoc />
    public override string ToString() => Data;

    public string TermsText => string.Join(" ", OriginalTerms);
    public string AnagramText => string.Join(" ", AnagramWords.Select(x => x.Text));

    public string Data =>TermsText + " = " + AnagramText;

    /// <inheritdoc />
    public override int GetHashCode() => Data.GetHashCode();

    /// <inheritdoc />
    public virtual bool Equals(PartialAnagram? other)
    {
        if (other is null) return false;

        return Data == other.Data;
    }
}
