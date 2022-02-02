namespace ContinuousPrimate;

public record NormalSearchPhrase
    (ImmutableList<PhraseComponent> Components, ImmutableList<int> Indexes) : SearchPhrase(Components)
{
    /// <inheritdoc />
    public override PartialAnagram TryMake(IReadOnlyList<Word> originalTerms, ImmutableList<Word> wordsSoFar)
    {
        var words = Indexes.Select(i => wordsSoFar[i]).ToList();
        return new PartialAnagram(originalTerms, words);
    }
}

public abstract record SearchPhrase(ImmutableList<PhraseComponent> Components) : SearchNode
{
    public abstract PartialAnagram? TryMake(IReadOnlyList<Word> originalTerms, ImmutableList<Word> wordsSoFar);

    /// <summary>
    /// Check that the words match the components
    /// </summary>
    public virtual bool CheckWords(IReadOnlyList<Word> originalTerms, ImmutableList<Word> words)
    {
        if (words.Count != Components.Count) return false;

        foreach (var (first, second) in words.Zip(Components))
        {
            if (!second.IsValidWord(first)) return false;
        }

        if (originalTerms.Select(x=>x.Text).Intersect(words.Select(x => x.Text), StringComparer.OrdinalIgnoreCase).Any())//Don't allow duplicate words
            return false;

        return true;
    }

    /// <inheritdoc />
    public override IEnumerable<PartialAnagram> FindAnagrams(IReadOnlyList<Word> originalTerms,
        AnagramKey remainingKey, ImmutableList<Word> wordsSoFar, WordDict wordDict)
    {
        if (remainingKey.IsEmpty())
        {
            if (!CheckWords(originalTerms, wordsSoFar))
                yield break;

            var r = TryMake(originalTerms, wordsSoFar);
            if (r is not null)
                yield return r;
        }
    }
}

public record IndefiniteArticleAComponent(int NextWordIndex) : FixedStringComponent(new Word("A",
    "Signifying one or any, but less emphatically.", WordType.Other))
{
    /// <inheritdoc />
    public override bool AllowPrecedingNodes(ImmutableList<Word> wordsSoFar)
    {
        return !Vowels.Contains(wordsSoFar[NextWordIndex].Text.First());
    }

    private static readonly IReadOnlySet<char> Vowels = new HashSet<char>()
    {
        'a', 'e', 'i', 'o', 'u',
        'A', 'E', 'I', 'O', 'U', 
        //Do not include 'H'
    };
}

public record IndefiniteArticleAnComponent(int NextWordIndex) : FixedStringComponent(new Word("An",
    "Signifying one or any, but less emphatically.", WordType.Other))
{
    /// <inheritdoc />
    public override bool AllowPrecedingNodes(ImmutableList<Word> wordsSoFar)
    {
        return Vowels.Contains(wordsSoFar[NextWordIndex].Text.First());
    }

    private static readonly IReadOnlySet<char> Vowels = new HashSet<char>()
    {
        'a', 'e', 'i', 'o', 'u',
        'A', 'E', 'I', 'O', 'U', 
        'h', 'H'//For this, H is a vowel
    };
}


public record FixedStringComponent(AnagramKey Key, Word Word) : PhraseComponent
{

    /// <inheritdoc />
    public override bool AllowPrecedingNodes(ImmutableList<Word> wordsSoFar) => true;

    public FixedStringComponent(Word word) : this(AnagramKey.Create(word.Text), word){}

    /// <inheritdoc />
    public override bool IsValidWord(Word word) => Word.Text.Equals(Word.Text);

    public override IEnumerable<(AnagramKey RemainingKey, Word Word)> GetPossibleWords(AnagramKey remainingKey,
        WordDict wordDict, bool useWholeKey)
    {
        if (useWholeKey)
        {
            if (remainingKey.Equals(Key))
                yield return (AnagramKey.Empty, Word);
        }
        else
        {
            if (remainingKey.Text.Length > Key.Text.Length)
            {
                var sub = remainingKey.TrySubtract(Key);
                if (sub is not null)
                    yield return (sub.Value, Word);
            }
        }
    }
}

public record WordComponent(WordType WordType) : PhraseComponent
{
    /// <inheritdoc />
    public override bool IsValidWord(Word word) => word.WordType == WordType;

    /// <inheritdoc />
    public override bool AllowPrecedingNodes(ImmutableList<Word> wordsSoFar) => true;

    public override IEnumerable<(AnagramKey RemainingKey, Word Word)> GetPossibleWords(AnagramKey remainingKey,
        WordDict wordDict,
        bool useWholeKey)
    {
        if (useWholeKey)
        {

            foreach (var w in wordDict.Tables[WordType].Lookup[remainingKey])
            {
                yield return (AnagramKey.Empty, w);
            }
        }
        else
        {
            if (remainingKey.Text.Length > 3) //Ignore words shorter than three letters
            {
                //This is the slow bit
                foreach (var (anagramKey, word) in wordDict.Tables[WordType].List)
                {
                    var sub = remainingKey.TrySubtract(anagramKey);
                    if (sub is not null) yield return (sub.Value, word);
                }
            }
        }
    }
}

public abstract record PhraseComponent
{
    public abstract bool IsValidWord(Word word);

    public abstract bool AllowPrecedingNodes(ImmutableList<Word> wordsSoFar);

    public abstract IEnumerable<(AnagramKey RemainingKey, Word Word)> GetPossibleWords(AnagramKey remainingKey,
        WordDict wordDict, bool useWholeKey);
}



public record ParentNode(ImmutableList<SearchNode> NextNodes) : SearchNode
{
    public override IEnumerable<PartialAnagram> FindAnagrams(IReadOnlyList<Word> originalTerms,
        AnagramKey remainingKey, ImmutableList<Word> wordsSoFar,
        WordDict wordDict) =>
        NextNodes
            .SelectMany(x => x.FindAnagrams(originalTerms, remainingKey, wordsSoFar, wordDict));
    
}

public record ComponentNode(PhraseComponent Component, SearchNode Child) : SearchNode
{

    /// <inheritdoc />
    public override IEnumerable<PartialAnagram> FindAnagrams(IReadOnlyList<Word> originalTerms,
        AnagramKey remainingKey, ImmutableList<Word> wordsSoFar, WordDict wordDict)
    {

        if (Component.AllowPrecedingNodes(wordsSoFar))
        {
            foreach (var (newRemainingKey, extraWord) in Component.GetPossibleWords(remainingKey, wordDict, Child is SearchPhrase))
            {
                var newWordsSoFar = wordsSoFar.Add(extraWord);

                foreach (var anagram in Child.FindAnagrams(originalTerms, newRemainingKey, newWordsSoFar, wordDict))
                    yield return anagram;
            }
        }
    }
}

public abstract record SearchNode
{
    public static SearchNode Default { get; } = CreateGraph(SearchPhrases.Default.ToList(), 0);
    public static SearchNode Name { get; } = CreateGraph(SearchPhrases.Name.ToList(), 0);

    
    public abstract IEnumerable<PartialAnagram> FindAnagrams(IReadOnlyList<Word> originalTerms,
        AnagramKey remainingKey, ImmutableList<Word> wordsSoFar, WordDict wordDict);

    public static SearchNode CreateGraph(IReadOnlyList<SearchPhrase> phrases, int level)
    {

        if (phrases.Count == 1 && level >= phrases.Single().Components.Count)
            return phrases.Single();

        var groupings = phrases.GroupBy(x => x.Components[level]);

        var componentNodes = groupings
            .Select(g =>
            {
                var childGraph = g.ToList();
                
                return new ComponentNode(g.Key, CreateGraph(childGraph, level + 1));
            })
            .ToImmutableList<SearchNode>();

        if (componentNodes.Count == 1)
            return componentNodes.Single();
        else
        {
            return new ParentNode(componentNodes);
        }
        
    }
}

public static class SearchPhrases
{

    public static IEnumerable<SearchPhrase> Name
    {
        get
        {
            yield return new NormalSearchPhrase(
                new PhraseComponent[] { new WordComponent(WordType.LastName), new WordComponent(WordType.FirstName) }.ToImmutableList(),
                new List<int>() { 1, 0 }.ToImmutableList());
        }
    }

    public static IEnumerable<SearchPhrase> Default
    {
        get
        {
            yield return new NormalSearchPhrase(
                new PhraseComponent[] { new WordComponent(WordType.Adjective), new WordComponent(WordType.Noun) }.ToImmutableList(),
                new List<int>() { 0, 1 }.ToImmutableList());

            yield return
                new NormalSearchPhrase(
                    new PhraseComponent[]{
                        new WordComponent(WordType.Adjective),
                        new IndefiniteArticleAnComponent(0),
                        new WordComponent(WordType.Noun)}.ToImmutableList(),
                    new[] { 1, 0, 2 }.ToImmutableList()
                );

            yield return
                new NormalSearchPhrase(
                    new PhraseComponent[]{
                        new WordComponent(WordType.Adjective),
                        new IndefiniteArticleAComponent(0),
                        new WordComponent(WordType.Noun)}.ToImmutableList(),
                    new[] { 1, 0, 2 }.ToImmutableList()
                    );

            yield return
                new NormalSearchPhrase(
                    new PhraseComponent[]{
                        new WordComponent(WordType.Adjective),
                        new FixedStringComponent(new Word("The","A word placed before nouns to limit or individualize their meaning.", WordType.Other )),
                        new WordComponent(WordType.Noun)}.ToImmutableList(),
                    new[] { 1, 0, 2 }.ToImmutableList()
                    );

            yield return
                new NormalSearchPhrase(
                    new PhraseComponent[]{
                        new WordComponent(WordType.Adjective),
                        new FixedStringComponent(new Word("One","Denoting a person or thing conceived or spoken of indefinitely.", WordType.Other )),
                        new WordComponent(WordType.Noun)}.ToImmutableList(),
                    new[] { 1, 0, 2 }.ToImmutableList()
                    );

            yield return new NormalSearchPhrase(
                new PhraseComponent[] { new WordComponent(WordType.Adverb), new WordComponent(WordType.Verb) }.ToImmutableList(),
                new List<int>() { 1, 0 }.ToImmutableList());


            yield return new NormalSearchPhrase(
                new PhraseComponent[] { new WordComponent(WordType.Adverb), 
                    new FixedStringComponent(new Word("I","The word with which a speaker or writer denotes themself.", WordType.Other )),
                    new WordComponent(WordType.Verb)
                }.ToImmutableList(),
                new List<int>()
                {
                    1, 2, 0
                }.ToImmutableList());
        }
    }

}