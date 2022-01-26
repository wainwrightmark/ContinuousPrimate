using System.Text;

namespace ContinuousPrimate;
public static class NameSearch
{


    public static IEnumerable<PartialAnagram> Search(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return ArraySegment<PartialAnagram>.Empty;

        if (name.Contains(" ") || name.Length >= 15)
        {
            return FindAnagrams(name, NounLookup.Value, AdjectiveLookup.Value);
        }

        return FindNameAnagrams(name, FirstNameLookup.Value, NounLookup.Value, AdjectiveLookup.Value);
    }

    public static IEnumerable<PartialAnagram> GetPartialAnagrams(string name)
    {
        var r =
            FindNameAnagrams(name, FirstNameLookup.Value, NounLookup.Value, AdjectiveLookup.Value);
        
        return r;
    }


    public static IEnumerable<string> GetAllFirstNames => Wordlist.Names.Split('\n');
    public static IEnumerable<string> GetAllNouns => Wordlist.Nouns.Split('\n');
    public static IEnumerable<string> GetAllAdjectives => Wordlist.Adjectives.Split('\n');

    public static readonly Lazy<ILookup<AnagramKey, string>> FirstNameLookup = new(() => GetAllFirstNames.ToLookup(AnagramKey.Create));
    public static readonly Lazy<ILookup<AnagramKey, string>> NounLookup = new(() => GetAllNouns.ToLookup(AnagramKey.Create));
    public static readonly Lazy<ILookup<AnagramKey, string>> AdjectiveLookup = new(() => GetAllAdjectives.ToLookup(AnagramKey.Create));


    public static IEnumerable<PartialAnagram> FindAnagrams(
        string fullTerm,
        ILookup<AnagramKey, string> nouns,
        ILookup<AnagramKey, string> adjectives
    )
    {
        var key = AnagramKey.CreateCareful(fullTerm);

        foreach (var adjective in adjectives)
        {
            var remainder = key.TrySubtract(adjective.Key);
            if (remainder .HasValue)
            {

                var noun = nouns[remainder.Value].FirstOrDefault();

                if (noun is not null)
                {
                    yield return new PartialAnagram(fullTerm,
                        adjective.First() + " " + noun
                    );
                }
            }
        }
    }


    public static IEnumerable<PartialAnagram> FindNameAnagrams(
        string mainWord,
        ILookup<AnagramKey, string> names,
        ILookup<AnagramKey, string> nouns,
        ILookup<AnagramKey, string> adjectives)
    {
        var mainKey = AnagramKey.CreateCareful(mainWord);
        
        foreach (var name in names)
        {
            var combo = mainKey.Add(name.Key);
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
                        yield return new PartialAnagram(name.First()+ " " + mainWord,
                            adjective.First() + " " + noun
                        );
                        break; //now try a new name
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
        => new(new string(s.Trim().ToLowerInvariant().OrderBy(x => x).ToArray()));
    
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
            sb.Append(Text.Substring(i));
        if (j < other.Text.Length)
            sb.Append(other.Text.Substring(j));

        return new(sb.ToString());
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
                sb.Append(Text.Substring(thisIndex));

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

        return new AnagramKey(sb.ToString());
    }
    
}




public record PartialAnagram(string FullName, string Anagram);
