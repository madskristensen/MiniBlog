using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

public class PostHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
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
            EditPost(id, context.Request.Form["title"], context.Request.Form["content"]);
        }
        else if (mode == "publish")
        {
            PublishPost(id, bool.Parse(context.Request.Form["publish"]));
        }
    }

    private void PublishPost(string id, bool publish)
    {
        Post post = Post.Posts.First(p => p.ID == id);
        post.IsPublished = publish;
        post.Save();
    }

    private void DeletePost(string id)
    {
        Post post = Post.Posts.First(p => p.ID == id);
        post.Delete();
    }

    private void EditPost(string id, string title, string content)
    {
        Post post = Post.Posts.FirstOrDefault(p => p.ID == id);

        if (post != null)
        {
            post.Title = title;
            post.Content = content;
        }
        else
        {
            post = new Post() { Title = title, Content = content, Slug = CreateSlug(title) };
            HttpContext.Current.Response.Write(post.Url);
        }

        SaveImagesToDisk(post);

        post.Save();
    }

    private void SaveImagesToDisk(Post post)
    {
        foreach (Match match in Regex.Matches(post.Content, "src=\"(data:([^\"]+))\""))
        {
            string extension = Regex.Match(match.Value, "data:image/([a-z]+);base64").Groups[1].Value;
     
            byte[] bytes = ConvertToBytes(match.Groups[1].Value);
            string path = Blog.SaveFileToDisk(bytes, extension);
            
            string image = string.Format("src=\"{0}\" alt=\"\" /", path);

            post.Content = post.Content.Replace(match.Value, image);
        }
    }

    private byte[] ConvertToBytes(string base64)
    {
        int index = base64.IndexOf("base64,", StringComparison.Ordinal) + 7;
        return Convert.FromBase64String(base64.Substring(index));
    }

    public static string CreateSlug(string title)
    {
        title = title.ToLowerInvariant().Replace(" ", "-");
        title = Regex.Replace(title, @"([^0-9a-z-])", string.Empty);

        if (Post.Posts.Any(p => string.Equals(p.Slug, title)))
            throw new HttpException(409, "Already in use");

        return title.ToLowerInvariant();
    }

    public bool IsReusable
    {
        get { return false; }
    }
}