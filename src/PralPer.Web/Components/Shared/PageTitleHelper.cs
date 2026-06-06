using System.Globalization;

namespace PralPer.Web.Components.Shared;

/// <summary>Derives a human-friendly screen title from the last URL segment.</summary>
public static class PageTitleHelper
{
    public static string FromUri(string uri)
    {
        var path = new Uri(uri).AbsolutePath.TrimEnd('/');
        var slug = path.Length == 0 ? "Home" : path[(path.LastIndexOf('/') + 1)..];
        var words = slug.Replace('-', ' ');
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words);
    }
}
