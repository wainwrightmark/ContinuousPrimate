using System.Net;

namespace ContinuousPrimate.Blazor.Pages;

public static class SocialHelpers
{
    public const string MyURL = "https://wainwrightmark.github.io/ContinuousPrimate";

    public static string GetFacebookShareURL(PartialAnagram partialAnagram)
    {
        var quote = GetQuote(partialAnagram);
        var url = WebUtility.UrlEncode(MyURL);
        var quote1 = WebUtility.UrlEncode(quote);

        return $"https://www.facebook.com/sharer/sharer.php?u={url}&quote={quote1}&hashtag={WebUtility.UrlEncode("#")}ContinuousPrimate";
    }

    public static string GetTwitterShareUrl(PartialAnagram partialAnagram)
    {
        var quote = GetQuote(partialAnagram);
        var url = WebUtility.UrlEncode(MyURL);
        var quote1 = WebUtility.UrlEncode(quote);

        return $"https://twitter.com/intent/tweet?url=@{url}&text=@{quote1}&hashtags=ContinuousPrimate";
    }

    public static string GetQuote(PartialAnagram partialAnagram)
    {
        return $"If my child was called '{partialAnagram.TermsText}' they'd be {GetArticle(partialAnagram.AnagramText)} {partialAnagram.AnagramText}.";
    }

    private static string GetArticle(string s)
    {
        if (Vowels.Contains(s.First()))
            return "an";
        return "a";
    }

    private static readonly HashSet<char> Vowels = new()
    {
        'a','e','i','o','u', 'A', 'E', 'I','O','U'
    };
}