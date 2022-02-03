using System.Collections.Concurrent;
using MoreLinq.Experimental;
using MudBlazor;

namespace ContinuousPrimate.Blazor.Pages;

public partial class NameSearchComponent
{
    private MudTextField<string> _searchField;
    private MudTable<PartialAnagram> _table;

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
        _table.ReloadServerData();
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
            ResetRowsPerPage();
            _table.ReloadServerData();
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

    
    private Task SetFavourite(bool fav, PartialAnagram anagram)
    {
        if (fav) return AddDatabaseWord(anagram);
        else return RemoveDatabaseWord(anagram);
    }

    public SearchType SearchType { get; set; } = SearchType.Dynamic;

    public WordType OtherWordType { get; set; } = WordType.Other;

    public void ResetRowsPerPage()
    {
        _table.SetRowsPerPage(10);
    }

    public void IncreaseRowsPerPage()
    {
        _table.SetRowsPerPage(_table.RowsPerPage + 5);
    }
    
    private bool _isSearching;
    

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
        }
    }

    private int TotalItems { get; set; } = 0;

    private async Task<TableData<PartialAnagram>> ServerReload(TableState state)
    {
        if (string.IsNullOrWhiteSpace(TextValue) || _wordDict is null)
        {
            TotalItems = DatabaseWords.Count;
            return new TableData<PartialAnagram>()
            {
                Items = DatabaseWords.Skip(state.Page * state.PageSize).Take(state.PageSize).ToArray(),
                TotalItems = DatabaseWords.Count
            };
        }

        _isSearching = true;

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
        
        _isSearching = false;

        var items = result
            .Skip(state.Page * state.PageSize)
            .Take(state.PageSize)
            .ToArray();

        int totalItems;
        if (items.Length == state.PageSize)
            totalItems = (state.Page + 2) * state.PageSize;
        else
            totalItems = (state.Page * state.PageSize) + items.Length;
        TotalItems = totalItems;
        return new TableData<PartialAnagram>()
        {
            Items = items,
            TotalItems = totalItems
        };
    }


    private readonly
        ConcurrentDictionary<(string text, SearchType searchType, WordType wordType), IEnumerable<PartialAnagram>>
        _cache = new();
}