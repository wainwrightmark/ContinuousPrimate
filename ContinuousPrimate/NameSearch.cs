using System.Diagnostics;
using System.Text;
using MoreLinq.Experimental;
using WordDict = System.Collections.Generic.IReadOnlyDictionary<ContinuousPrimate.PartOfSpeech, System.Collections.Generic.IReadOnlyDictionary<ContinuousPrimate.AnagramKey, ContinuousPrimate.Word>>;

namespace ContinuousPrimate;



public record Word(string Text, string Gloss);


public static class WordListHelper
{
    public static Lazy<IEnumerable<(AnagramKey key, string name)>> CreateEnumerable(string data)
    {
        return new Lazy<IEnumerable<(AnagramKey key, string name)>>(()=>data
                .Split('\n', StringSplitOptions.TrimEntries)
                .Select(name => (AnagramKey.Create(name), name)).Memoize()
                ) ;
    }

    public static WordDict
        CreateFullWordDictionary(string data)
    {
        var sw = Stopwatch.StartNew();

        var dict = data.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(GetData)
            .GroupBy(x => x.partOfSpeech)
            .ToDictionary(x => x.Key
                , x =>
                    x.GroupBy(y=>y.anagramKey)
                        
                        
                        .ToDictionary(y => y.Key, 
                        y => y.First().word) as IReadOnlyDictionary<AnagramKey, Word>
            );

        Console.WriteLine(DateTime.Now + ": Word Dictionary Created " + sw.Elapsed);

        return dict;


        static (PartOfSpeech partOfSpeech, AnagramKey anagramKey, Word word) GetData(string line)
        {
            var terms = line.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var pos = GetPartOfSpeech(terms[0]);
            var word = terms[1];
            var key = new AnagramKey(terms[2]);
            var gloss = terms[3];

            return (pos, key, new Word(word, gloss));
        }
    }

    public static PartOfSpeech GetPartOfSpeech(string s)
    {
        if (s == "n") return PartOfSpeech.Noun;
        if (s == "j") return PartOfSpeech.Adjective;
        if (s == "a") return PartOfSpeech.Adverb;
        if (s == "v") return PartOfSpeech.Verb;
        return PartOfSpeech.Other;
    }
}

public static class NameSearch
{
    
    public static IEnumerable<PartialAnagram> Search(string name,
        IEnumerable<(AnagramKey key, string name)> firstNames,
        WordDict wordDictionary
        )
    {
        if(string.IsNullOrWhiteSpace(name))
            return ArraySegment<PartialAnagram>.Empty;

        if (name.Contains(" ") || name.Length >= 15)
        {
            return FindAnagrams(name, wordDictionary);
        }

        return FindNameAnagrams(name, firstNames, wordDictionary);
    }
    

    public static IEnumerable<PartialAnagram> FindAnagrams(
        string fullTerm,
        WordDict wordDictionary
    )
    {
        var key = AnagramKey.CreateCareful(fullTerm);
        var originalTerms = fullTerm.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var adjectives = wordDictionary[PartOfSpeech.Adjective];
        var nouns = wordDictionary[PartOfSpeech.Noun];


        foreach (var (adjectiveKey, adjective) in adjectives)
        {
            var remainder = key.TrySubtract(adjectiveKey);
            if (remainder .HasValue)
            {
                if (nouns.TryGetValue(remainder.Value, out var noun))
                {
                    yield return new PartialAnagram(originalTerms,
                        new List<Word>(){adjective, noun}
                    );
                }
            }
        }
    }


    public static IEnumerable<PartialAnagram> FindNameAnagrams(
        string mainWord,
        IEnumerable<(AnagramKey key, string word)> names,
        WordDict wordDictionary)
    {
        var mainKey = AnagramKey.CreateCareful(mainWord);
        var adjectives = wordDictionary[PartOfSpeech.Adjective];
        var nouns = wordDictionary[PartOfSpeech.Noun];


        foreach (var (key, firstName) in names)
        {
            var combo = mainKey.Add(key);
            foreach (var  (adjectiveKey, adjective)  in adjectives)
            {
                var remainder = combo.TrySubtract(adjectiveKey);
                if (remainder .HasValue)
                {
                    if(adjectiveKey == mainKey)continue; //Best to do this check here - we get an extra subtraction but skip a bunch of checks
                    if(remainder.Value == mainKey)continue;

                    if (nouns.TryGetValue(remainder.Value, out var noun))
                    {
                        yield return new PartialAnagram(
                            new List<string>(){firstName, mainWord},
                            new List<Word>(){adjective, noun}
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
