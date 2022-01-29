namespace ContinuousPrimate.Blazor.Pages;

public class LoadedData
{
    public LoadedData(IEnumerable<PartialAnagram> anagrams)
    {
        Anagrams = anagrams;
    }

    public IEnumerable<PartialAnagram> Anagrams { get; }

    public IEnumerable<PartialAnagram> TableElements => Anagrams.Skip((PageNumber - 1) * PageSize).Take(PageSize);


    public int? MaxPages { get; private set; } = null;

    private int _pageNumber = 1;

    public bool CanIncreasePageNumber => MaxPages is null || _pageNumber < MaxPages;

    public int PageNumber
    {
        get => _pageNumber;
        set
        {
            var any = Anagrams.Skip((value - 1) * PageSize).Take(PageSize).Any();

            if (!any)
            {
                MaxPages = (Anagrams.Count() / PageSize) + 1;
            }
            else
            {
                _pageNumber = value;
            }
        }
    }

    public int PageSize = 10;
}

