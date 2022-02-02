using System.IO.Compression;
namespace ContinuousPrimate.Blazor.Pages;

public static class DataLoading
{

    public static async Task<Lazy<WordDict>> GetWordDict(HttpClient client)
    {
        var text = await LoadText(client, "./Data/WordData.gzip");

        Console.WriteLine(DateTime.Now + $": Word Data Loaded - {text.Length} characters");

        var result = new Lazy<WordDict>(() => WordDict.Create(text));

        return result;
    }

    private static async Task<string> LoadText(HttpClient client, string uri)
    {
         var stream = await client.GetStreamAsync(uri);

         //await using var to = new MemoryStream();
         await using var gZipStream = new GZipStream(stream, CompressionMode.Decompress);
         //await gZipStream.CopyToAsync(to);

         var reader = new StreamReader(gZipStream);
         var text = await reader.ReadToEndAsync();
         return text;
    }
}
