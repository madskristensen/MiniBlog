<%@ WebHandler Language="C#" Class="Edit" %>

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

public class Edit : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string mode = context.Request.QueryString["mode"];
        string id = context.Request.Form["id"];
        string title = context.Request.Form["title"];
        string content = context.Request.Form["content"];

        if (!context.User.Identity.IsAuthenticated || context.Request.RequestType != "POST" || string.IsNullOrEmpty(mode))
            throw new HttpException(403, "No access");

        if (mode == "delete")
        {
            DeletePost(id);
        }
        else if (mode == "save")
        {
            EditPost(id, title, content);
        }
        else if (mode == "upload")
        {
            context.Response.Write("http://madskristensen.net/themes/standard/madskristensen.jpg");
        }
    }

    private void DeletePost(string id)
    {
        Post post = Post.Posts.FirstOrDefault(p => p.ID == id);
        
        if (post == null)
            throw new HttpException(404, "Not found");

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
            post = new Post();
            post.Slug = CreateSlug(title);
            post.Title = title;
            post.Content = content;            
            HttpContext.Current.Response.Write(post.Slug);
        }

        post.Save();
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