namespace ContinuousPrimate;

public record PartialAnagram(IReadOnlyList<Word> OriginalTerms, IReadOnlyList<Word> AnagramWords)
{
    /// <inheritdoc />
    public override string ToString() => Data;

    public string TermsText => string.Join(" ", OriginalTerms.Select(x=>x.Text));
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