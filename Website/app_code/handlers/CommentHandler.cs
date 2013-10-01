using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

public class CommentHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        Post post = Post.GetAllPosts().SingleOrDefault(p => p.ID == context.Request.Form["postId"]);

        if (post == null)
            throw new HttpException(404, "The post does not exist");

        string mode = context.Request.Form["mode"];

        if (mode == "save" && post.PubDate > DateTime.UtcNow.AddDays(-Blog.DaysToComment))
        {
            Save(context, post);
        }
        else if (mode == "delete")
        {
            Delete(context, post);
        }
    }

    private static void Save(HttpContext context, Post post)
    {
        string name = context.Request.Form["name"];
        string email = context.Request.Form["email"];
        string website = context.Request.Form["website"];
        string content = context.Request.Form["content"];

        Validate(name, email, content);

        Comment comment = new Comment()
        {
            Author = name.Trim(),
            Email = email.Trim(),
            Website = GetUrl(website),
            Ip = context.Request.UserHostAddress,
            UserAgent = context.Request.UserAgent,
            IsAdmin = context.User.Identity.IsAuthenticated,
            Content = HttpUtility.HtmlEncode(content.Trim()).Replace("\n", "<br />"),
        };

        post.Comments.Add(comment);
        post.Save();

        string wrapper = VirtualPathUtility.ToAbsolute("~/views/commentwrapper.cshtml") + "?postid=" + post.ID + "&commentid=" + comment.ID;
        context.Response.Write(wrapper);
    }

    private static void Validate(string name, string email, string content)
    {
        bool isName = !string.IsNullOrEmpty(name);
        bool isMail = !string.IsNullOrEmpty(email) && Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        bool isContent = !string.IsNullOrEmpty(content);

        if (!isName || !isMail || !isContent)
        {
            if (!isName)
                HttpContext.Current.Response.Write("Please enter a valid name");
            else if (!isMail)
                HttpContext.Current.Response.Write("Please enter a valid e-mail address");
            else if (!isContent)
                HttpContext.Current.Response.Write("Please enter a valid comment");

            HttpContext.Current.Response.StatusCode = 403;
            HttpContext.Current.Response.End();
        }
    }

    private static string GetUrl(string website)
    {
        if (!website.Contains("://"))
            website = "http://" + website;

        Uri url;
        if (Uri.TryCreate(website, UriKind.Absolute, out url))
            return url.ToString();

        return string.Empty;
    }

    private static void Delete(HttpContext context, Post post)
    {
        if (!context.User.Identity.IsAuthenticated)
            throw new HttpException(403, "No access");

        string commentId = context.Request.Form["commentId"];
        Comment comment = post.Comments.SingleOrDefault(c => c.ID == commentId);

        if (comment != null)
        {
            post.Comments.Remove(comment);
            post.Save();
        }
        else
        {
            throw new HttpException(404, "Comment could not be found");
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}