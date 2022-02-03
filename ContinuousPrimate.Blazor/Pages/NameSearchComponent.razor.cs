using System.Collections.Concurrent;
using MoreLinq.Experimental;
using MudBlazor;

namespace ContinuousPrimate.Blazor.Pages;

public partial class NameSearchComponent
{
    private MudTextField<string> _searchField;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Console.WriteLine(DateTime.Now + ": Component Initialized");

        await LoadWordData();
        await LoadDatabaseWords();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        await _searchField.FocusAsync();
    }

    private const string SavedWordsKey = "ContinuousPrimateSavedAnagrams";

    private async Task LoadDatabaseWords()
    {
        var data = await _localStorage.GetItemAsync<List<PartialAnagram>>(SavedWordsKey) ?? new List<PartialAnagram>();

        DatabaseWords.UnionWith(data);

        Console.WriteLine($"{DatabaseWords.Count} Database Words Found");

        DatabaseWordsData = new LoadedData(DatabaseWords);
        StateHasChanged();
    }

    private async Task LoadWordData()
    {
        if (_wordDict is null)
        {
            _wordDict = await DataLoading.GetWordDict(_httpClient);
        }
    }

    private static Lazy<WordDict>? _wordDict;


    private string _textValue = "";

    public string TextValue
    {
        get => _textValue;
        set
        {
            _textValue = value.Trim();
            Search();
        }
    }


    public async Task RemoveDatabaseWord(PartialAnagram pa)
    {
        if (DatabaseWords.Remove(pa))
            await _localStorage.SetItemAsync(SavedWordsKey, DatabaseWords.ToList());
    }

    public async Task AddDatabaseWord(PartialAnagram pa)
    {
        if (DatabaseWords.Add(pa))
            await _localStorage.SetItemAsync(SavedWordsKey, DatabaseWords.ToList());
    }

    public readonly HashSet<PartialAnagram> DatabaseWords = new();


    public LoadedData? MyData
    {
        get
        {
            if (_isSearching) return null;

            if (!string.IsNullOrWhiteSpace(TextValue))
                return SearchData;

            if (DatabaseWordsData is null || !DatabaseWordsData.Anagrams.Any())
                return null;

            return DatabaseWordsData;
        }
    }

    public LoadedData? DatabaseWordsData;

    public LoadedData? SearchData { get; private set; }

    private Task SetFavourite(bool fav, PartialAnagram anagram)
    {
        if (fav) return AddDatabaseWord(anagram);
        else return RemoveDatabaseWord(anagram);
    }

    public SearchType SearchType { get; set; } = SearchType.Dynamic;

    public WordType OtherWordType { get; set; } = WordType.Other;

    public string HelperText = "Type your last name (or whatever)";

    private bool _isSearching;

    public async Task Search()
    {
        if (!_isSearching && _wordDict is not null)
        {
            HelperText = "Searching";
            _isSearching = true;
            SearchData = null;

            StateHasChanged();
            await Task.Delay(1);

            var result =
                _cache.GetOrAdd(
                    (TextValue, SearchType, OtherWordType)
                    , t => NameSearch.Search(t.text,
                        t.searchType,
                        t.wordType,
                        _wordDict.Value
                    ).Memoize());

            HelperText = "";
            SearchData = new LoadedData(result);
            _isSearching = false;
            StateHasChanged();
        }
    }

    public async Task ShowSettings()
    {
        var parameters = new DialogParameters
        {
            { nameof(SearchType), SearchType },
            { nameof(OtherWordType), OtherWordType }
        };

        var options = new DialogOptions() { FullWidth = true, DisableBackdropClick = true};

        var dialog = DialogService.Show<SettingsDialog>("Settings", parameters, options);

        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            var resultData = ((SearchType searchType, WordType wordType)) result.Data;

            SearchType = resultData.searchType;
            OtherWordType = resultData.wordType;

            Search();
        }
    }

    private readonly
        ConcurrentDictionary<(string text, SearchType searchType, WordType wordType), IEnumerable<PartialAnagram>>
        _cache = new();
}