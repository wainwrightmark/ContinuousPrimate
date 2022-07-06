using System.Numerics;

namespace ContinuousPrimate;


public readonly record struct AnagramKey(byte Length, BigInteger BigInt)
{
    /// <inheritdoc />
    public override string ToString() => GetText();

    public string GetText()
    {
        var sb = new StringBuilder();

        var rem = BigInt;

        if (rem > 1)
        {
            for (var i = 0; i < PrimesByLetter.Length; i++)
            {
                var prime = PrimesByLetter[i];

                while (rem % prime == 0)
                {
                    var c = (char)('a' + i);
                    sb.Append(c);
                    rem /= prime;

                    if (rem == 1)
                        return sb.ToString();
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Create from a piece of text
    /// </summary>
    public static AnagramKey Create(string s)
        => CreateCareful(s);

    /// <summary>
    /// Create from a piece of text that might contain special characters
    /// </summary>
    public static AnagramKey CreateCareful(string s)
    {
        var chars = s.Trim().ToLowerInvariant().Where(char.IsLetter);

        BigInteger bigInt = 1;
        byte length = 0;
        foreach (var c in chars)
        {
            var i = c - 'a';
            var prime = PrimesByLetter[i];
            bigInt *= prime;
            length++;
        }

        return new AnagramKey(length, bigInt);
    }

    /// <summary>
    /// The first 26 primes
    /// </summary>
    private static readonly int[] PrimesBySize = {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,101
    };

    private static readonly char[] LettersByFrequency = {
        'e','t','a','i','n',
        'o','s','h','r','d',
        'l','u','c','m','f',
        'w','y','g','p','b',
        'v','k','q','j','x',
        'z'
    };

    /// <summary>
    /// Indexing this array by a letter index will give the prime for this char
    /// </summary>
    private static readonly int[] PrimesByLetter =
        LettersByFrequency.Select((c, i) => (c, i))
            .OrderBy(x => x.c).Select(x => PrimesBySize[x.i]).ToArray();


    
    
    public bool IsEmpty() => Length == 0;

    public static AnagramKey Empty { get; } = CreateCareful("");

    public AnagramKey Add(AnagramKey other)
    {
        var newLen = (byte) (Length + other.Length);
        var newBigint = BigInt * other.BigInt;

        return new AnagramKey(newLen, newBigint);
    }

    public AnagramKey? TrySubtract(AnagramKey other)
    {
        if (other.Length > Length)
            return null; //Impossible

        if (other.IsEmpty())
            return this; //No change

        if(BigInt % other.BigInt != 0)
            return null; //Impossible

        var newBigInt = BigInt / other.BigInt;
        var newLen = (byte) (Length - other.Length);
        
        return new AnagramKey(newLen, newBigInt);
    }
}