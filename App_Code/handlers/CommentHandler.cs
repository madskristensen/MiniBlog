using System.Linq;
using System.Web;
public class CommentHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        Post post = Post.Posts.SingleOrDefault(p => p.ID == context.Request.Form["postId"]);

        if (post == null)
            throw new HttpException(404, "The post does not exist");

        string mode = context.Request.QueryString["mode"];
        if (mode == "save")
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
        string email = context.Request.Form["email"];
        string name = context.Request.Form["name"];
        string content = context.Request.Form["content"];

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(content))
            throw new HttpException(500, "Comment not valid");

        Comment comment = new Comment()
        {
            Author = name,
            Email = email,
            Content = HttpUtility.HtmlEncode(content).Replace("\n", "<br />"),
        };

        post.Comments.Add(comment);
        post.Save();
        context.Response.Write(comment.ID);
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