

namespace ContinuousPrimate;

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

    public bool IsEmpty() => Text.Length == 0;

    public static AnagramKey Empty { get; } = new ("");

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