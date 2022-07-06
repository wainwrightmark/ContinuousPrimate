

namespace ContinuousPrimate;


public readonly record struct AnagramKey(byte Length, int HashCode, byte[] Inner)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return GetText();
    }

    /// <inheritdoc />
    public override int GetHashCode() => HashCode;


    public bool Equals(AnagramKey other)
    {
        if(ReferenceEquals(Inner, other.Inner)) return true;

        if (Length == other.Length && HashCode == other.HashCode && Inner.SequenceEqual(other.Inner))
            return true;
        return false;
    }

    public string GetText()
    {
        var sb = new StringBuilder();

        for (var i = 0; i < Inner.Length; i++)
        {
            var num = Inner[i];
            if (num > 0)
            {
                var c = (char) ('a' + i);

                sb.Append(c, num);
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
        var groups = s.Trim().ToLowerInvariant().Where(char.IsLetter)
            .GroupBy(x => x)
            .OrderBy(x => x.Key);

        byte[] inner = new byte[26];
        byte length = 0;
        foreach (var group in groups)
        {
            var k = group.Key - 'a';
            var n = (byte) Math.Min(255,group.Count());

            inner[k] = n;
            length += n;
        }

        return new AnagramKey(length, MakeHashCode(inner), inner);
    }

    /// <summary>
    /// The first 26 primes
    /// </summary>
    private static readonly int[] Primes = new[]
    {
        2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,101
    };

    private static int MakeHashCode(byte[] inner)
    {
        unchecked
        {
            var hashcode = 1;
            for (var i = 0; i < inner.Length; i++)
            {
                var n = inner[i];
                while (n > 0)
                {
                    n--;
                    hashcode *= Primes[i];
                }
            }
            return hashcode;
        }
    }
    public bool IsEmpty() => Length == 0;

    public static AnagramKey Empty { get; } = CreateCareful("");

    public AnagramKey Add(AnagramKey other)
    {
        if(other.IsEmpty())return this;
        if (IsEmpty()) return other;

        var newArr = new byte[26];

        for (int i = 0; i < Inner.Length; i++)
        {
            newArr[i] = (byte) (Inner[i] + other.Inner[i]);
        }

        var newLen = (byte) (Length + other.Length);

        return new AnagramKey(newLen,MakeHashCode(newArr), newArr);
    }

    public AnagramKey? TrySubtract(AnagramKey other)
    {
        if (other.Length > Length)
            return null; //Impossible

        if (other.IsEmpty())
            return this; //No change

        var newArr = new byte[26];

        for (var i = 0; i < Inner.Length; i++)
        {
            if (Inner[i] < other.Inner[i])
                return null;

            newArr[i] = (byte) (Inner[i] - other.Inner[i]);
        }

        var newLen = (byte) (Length - other.Length);

        return new AnagramKey(newLen,MakeHashCode(newArr), newArr);
    }

}

//public readonly record struct AnagramKey(string Text)
//{
//    /// <summary>
//    /// Create from a piece of text
//    /// </summary>
//    public static AnagramKey Create(string s)
//        => new(new string(s.ToLowerInvariant().OrderBy(x => x).ToArray()));
    
//    /// <summary>
//    /// Create from a piece of text that might contain special characters
//    /// </summary>
//    public static AnagramKey CreateCareful(string s)
//        => new(new string(s.Trim().ToLowerInvariant().Where(char.IsLetter).OrderBy(x => x).ToArray()));

//    public bool IsEmpty() => Text.Length == 0;

//    public static AnagramKey Empty { get; } = new ("");

//    public AnagramKey Add(AnagramKey other)
//    {
//        int i = 0, j = 0;
//        StringBuilder sb = new();

//        while (i < Text.Length && j < other.Text.Length)
//        {
//            if (Text[i] < other.Text[j])
//                sb.Append(Text[i++]);
//            else
//                sb.Append(other.Text[j++]);
//        }

//        if (i < Text.Length)
//            sb.Append(Text[i..]);
//        if (j < other.Text.Length)
//            sb.Append(other.Text[j..]);

//        return new AnagramKey(sb.ToString());
//    }

//    public AnagramKey? TrySubtract(AnagramKey other)
//    {
//        if (other.Text.Length > Text.Length)
//            return null; //Impossible

//        if (string.IsNullOrEmpty(other.Text))
//            return this; //No change

//        var sb = new StringBuilder();
        
//        var otherIndex = 0;
//        for (var thisIndex = 0; thisIndex < Text.Length; thisIndex++)
//        {
//            if (otherIndex >= other.Text.Length)
//            {
//                //We've finished consuming the other string
//                sb.Append(Text[thisIndex..]);

//                break;
//            }
            
//            var thisChar = Text[thisIndex];
//            var otherChar = other.Text[otherIndex];

//            if (thisChar == otherChar) //Same character in both - ignore it
//            {
//                otherIndex++;
//            }
//            else if(thisChar < otherChar)//Extra character in this
//            {
//                sb.Append(thisChar);
//            }
//            else
//            {
//                return null; //There is a char in other which isn't in this
//            }
//        }

//        if (otherIndex < other.Text.Length) return null; //There are remaining characters in other we shouldn't return

//        return new AnagramKey(sb.ToString());
//    }
    
//}