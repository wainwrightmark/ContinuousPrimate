namespace ContinuousPrimate;

public record SearchPhrase(IReadOnlyList<SearchPhrase.PhraseComponent> Components)
{

    public abstract record PhraseComponent;

}