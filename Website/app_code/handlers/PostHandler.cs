using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

public class PostHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        Blog.ValidateToken(context);

        if (!context.User.Identity.IsAuthenticated)
            throw new HttpException(403, "No access");

        string mode = context.Request.QueryString["mode"];
        string id = context.Request.Form["id"];

        if (mode == "delete")
        {
            DeletePost(id);
        }
        else if (mode == "save")
        {
            EditPost(id, context.Request.Form["title"], context.Request.Form["excerpt"], context.Request.Form["content"], bool.Parse(context.Request.Form["isPublished"]), context.Request.Form["categories"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
        }
    }

    private void DeletePost(string id)
    {
        Post post = Storage.GetAllPosts().FirstOrDefault(p => p.ID == id);

        if (post == null)
            throw new HttpException(404, "The post does not exist");

        Storage.Delete(post);
    }

    private void EditPost(string id, string title, string excerpt, string content, bool isPublished, string[] categories)
    {
        Post post = Storage.GetAllPosts().FirstOrDefault(p => p.ID == id);

        if (post != null)
        {
            post.Title = title;
            post.Excerpt = excerpt;
            post.Content = content;
            post.Categories = categories;
        }
        else
        {
            post = new Post() { Title = title, Excerpt = excerpt, Content = content, Slug = CreateSlug(title), Categories = categories };
            HttpContext.Current.Response.Write(post.Url);
        }

        SaveFilesToDisk(post);

        post.IsPublished = isPublished;
        Storage.Save(post);
    }

    private void SaveFilesToDisk(Post post)
    {
        foreach (Match match in Regex.Matches(post.Content, "(src|href)=\"(data:([^\"]+))\"(>.*?</a>)?"))
        {
            string extension = string.Empty;
            string filename = string.Empty;

            // Image
            if (match.Groups[1].Value == "src")
            {
                extension = Regex.Match(match.Value, "data:([^/]+)/([a-z]+);base64").Groups[2].Value;
            }
            // Other file type
            else
            {
                // Entire filename
                extension = Regex.Match(match.Value, "data:([^/]+)/([a-z0-9+-.]+);base64.*\">(.*)</a>").Groups[3].Value;
            }

            byte[] bytes = ConvertToBytes(match.Groups[2].Value);
            string path = Blog.SaveFileToDisk(bytes, extension);

            string value = string.Format("src=\"{0}\" alt=\"\" ", path);

            if (match.Groups[1].Value == "href")
                value = string.Format("href=\"{0}\"", path);

            Match m = Regex.Match(match.Value, "(src|href)=\"(data:([^\"]+))\"");
            post.Content = post.Content.Replace(m.Value, value);
        }
    }

    private byte[] ConvertToBytes(string base64)
    {
        int index = base64.IndexOf("base64,", StringComparison.Ordinal) + 7;
        return Convert.FromBase64String(base64.Substring(index));
    }

    public static string CreateSlug(string title)
    {
        title = title.ToLowerInvariant().Replace(" ", "-").Replace("#", "");
        title = RemoveDiacritics(title);

        if (Storage.GetAllPosts().Any(p => string.Equals(p.Slug, title, StringComparison.OrdinalIgnoreCase)))
            throw new HttpException(409, "Already in use");

        return title.ToLowerInvariant();
    }

    static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public bool IsReusable
    {
        get { return false; }
    }
}