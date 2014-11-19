using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Security;

public class Comment
{
    private static readonly Regex _linkRegex = new Regex("((http://|https://|www\\.)([A-Z0-9.\\-]{1,})\\.[0-9A-Z?;~&%\\(\\)#,=\\-_\\./\\+]{2,}[0-9A-Z?~&%#=\\-_/\\+])", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private const string Link = "<a href=\"{0}{1}\" rel=\"nofollow\">{2}</a>";

    public Comment()
    {
        ID = Guid.NewGuid().ToString();
        PubDate = DateTime.UtcNow;
    }

    public string ID { get; set; }
    public string Author { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
    public string Content { get; set; }
    public DateTime PubDate { get; set; }
    public string Ip { get; set; }
    public string UserAgent { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsApproved { get; set; }

    public string GravatarUrl(int size)
    {
        var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(Email.ToLowerInvariant(), "MD5").ToLower();

        return string.Format("http://gravatar.com/avatar/{0}?s={1}&d=mm", hash, size);
    }

    public string ContentWithLinks()
    {
        return _linkRegex.Replace(Content, new MatchEvaluator(Evaluator));
    }

    private static string Evaluator(Match match)
    {
        var info = CultureInfo.InvariantCulture;
        return string.Format(info, Link, !match.Value.Contains("://") ? "http://" : string.Empty, match.Value, ShortenUrl(match.Value, 50));
    }

    private static string ShortenUrl(string url, int max)
    {
        if (url.Length <= max)
        {
            return url;
        }

        // Remove the protocal
        var startIndex = url.IndexOf("://");
        if (startIndex > -1)
        {
            url = url.Substring(startIndex + 3);
        }

        if (url.Length <= max)
        {
            return url;
        }

        // Compress folder structure
        var firstIndex = url.IndexOf("/") + 1;
        var lastIndex = url.LastIndexOf("/");
        if (firstIndex < lastIndex)
        {
            url = url.Remove(firstIndex, lastIndex - firstIndex);
            url = url.Insert(firstIndex, "...");
        }

        if (url.Length <= max)
        {
            return url;
        }

        // Remove URL parameters
        var queryIndex = url.IndexOf("?");
        if (queryIndex > -1)
        {
            url = url.Substring(0, queryIndex);
        }

        if (url.Length <= max)
        {
            return url;
        }

        // Remove URL fragment
        var fragmentIndex = url.IndexOf("#");
        if (fragmentIndex > -1)
        {
            url = url.Substring(0, fragmentIndex);
        }

        if (url.Length <= max)
        {
            return url;
        }

        // Compress page
        firstIndex = url.LastIndexOf("/") + 1;
        lastIndex = url.LastIndexOf(".");
        if (lastIndex - firstIndex > 10)
        {
            var page = url.Substring(firstIndex, lastIndex - firstIndex);
            var length = url.Length - max + 3;
            if (page.Length > length)
            {
                url = url.Replace(page, string.Format("...{0}", page.Substring(length)));
            }
        }

        return url;
    }
}