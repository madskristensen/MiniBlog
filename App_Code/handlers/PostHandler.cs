using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

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
            string fileName = "/posts/files/" + Guid.NewGuid() + "." + extension;
            string image = string.Format("src=\"{0}\" alt=\"\" /", fileName);

            UploadImage(post.ID, match.Groups[1].Value, fileName);

            post.Content = post.Content.Replace(match.Value, image);
        }
    }

    private void UploadImage(string id, string data, string name)
    {
        Post post = Post.Posts.First(p => p.ID == id);

        string relative = "~" + name;
        string file = HostingEnvironment.MapPath(relative);
        int index = data.IndexOf("base64,", StringComparison.Ordinal) + 7;

        byte[] imageBytes = Convert.FromBase64String(data.Substring(index));
        File.WriteAllBytes(file, imageBytes);
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