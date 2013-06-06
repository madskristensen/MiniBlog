<%@ WebHandler Language="C#" Class="Edit" %>

using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

public class Edit : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        if (!context.User.Identity.IsAuthenticated || context.Request.RequestType != "POST")
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
        else if (mode == "upload")
        {
            UploadImage(id, context.Request.Form["data"], context.Request.Form["name"]);
        }
    }

    private void UploadImage(string id, string data, string name)
    {
        Post post = Post.Posts.First(p => p.ID == id);

        string file = Path.Combine(HttpContext.Current.Server.MapPath("~/Posts/"), post.Slug, name);
        int index = data.IndexOf("base64,", StringComparison.Ordinal) + 7;
        byte[] imageBytes = Convert.FromBase64String(data.Substring(index));
        File.WriteAllBytes(file, imageBytes);

        HttpContext.Current.Response.Write("/posts/" + post.Slug + "/" + name);
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
            HttpContext.Current.Response.Write(post.Slug);
        }

        SaveImagesToDisk(post);
        
        post.Save();
    }

    private void SaveImagesToDisk(Post post)
    {
        foreach (Match match in Regex.Matches(post.Content, "src=\"(data:([^\"]+))\""))
        {
            string extension = Regex.Match(match.Value, "data:image/([a-z]+);base64").Groups[1].Value;
            string fileName = Guid.NewGuid() + "." + extension;
            string image = string.Format("src=\"/posts/{0}/{1}\" alt=\"\" /", post.Slug, fileName);
            UploadImage(post.ID, match.Groups[1].Value, fileName);

            post.Content = post.Content.Replace(match.Value, image);
        }        
    }

    private string CreateSlug(string title)
    {
        title = title.ToLowerInvariant().Replace(" ", "-");
        title = Regex.Replace(title, @"([^0-9a-z-])", string.Empty);

        if (Post.Posts.Any(p => string.Equals(p.Slug, title)))
            throw new HttpException(409, "Already in use");

        return title;
    }

    public bool IsReusable
    {
        get { return false; }
    }
}