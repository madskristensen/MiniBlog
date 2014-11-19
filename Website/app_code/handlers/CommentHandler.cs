using System;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.WebPages;

public class CommentHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        Post post = Storage.GetAllPosts().FirstOrDefault(p => p.ID == context.Request["postId"]);

        if (post == null)
            throw new HttpException(404, "The post does not exist");

        string mode = context.Request["mode"];

        if (mode == "save" && context.Request.HttpMethod == "POST")
        {
            Save(context, post);
        }
        else if (mode == "delete")
        {
            Delete(context, post);
        }
        else if (mode == "approve")
        {
            Approve(context, post);
        }
    }

    private static void Save(HttpContext context, Post post)
    {
        Blog.ValidateToken(context);

        if (!post.AreCommentsOpen(new HttpContextWrapper(context)))
            throw new HttpException(403, "The data token doesn't match or comments are closed");

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
            IsApproved = !Blog.ModerateComments,
        };

        post.Comments.Add(comment);
        Storage.Save(post);

        if (!context.User.Identity.IsAuthenticated)
        {
            MailMessage mail = GenerateEmail(comment, post, context.Request);
            System.Threading.ThreadPool.QueueUserWorkItem((s) => SendEmail(mail));
        }

        RenderComment(context, comment);
    }

    private static void RenderComment(HttpContext context, Comment comment)
    {
        var page = (WebPage)WebPageBase.CreateInstanceFromVirtualPath("~/themes/" + Blog.Theme + "/comment.cshtml");
        page.Context = new HttpContextWrapper(context);
        page.ExecutePageHierarchy(new WebPageContext(page.Context, page: null, model: comment), context.Response.Output);
    }

    private static void SendEmail(MailMessage mail)
    {
        try
        {
            using (SmtpClient client = new SmtpClient())
            {
                client.Send(mail);
                mail.Dispose();
            }
        }
        catch
        { }
    }

    private static MailMessage GenerateEmail(Comment comment, Post post, HttpRequest request)
    {
        MailMessage mail = new MailMessage();
        mail.From = new MailAddress(comment.Email, comment.Author);
        mail.ReplyToList.Add(comment.Email);
        mail.To.Add(ConfigurationManager.AppSettings.Get("blog:email"));
        mail.Subject = "Blog comment: " + post.Title;
        mail.IsBodyHtml = true;

        string absoluteUrl = request.Url.Scheme + "://" + request.Url.Authority;
        string deleteUrl = absoluteUrl + request.RawUrl + "?postId=" + post.ID + "&commentId=" + comment.ID + "&mode=delete";
        string approveUrl = absoluteUrl + request.RawUrl + "?postId=" + post.ID + "&commentId=" + comment.ID + "&mode=approve";
        mail.Body = "<div style=\"font: 11pt/1.5 calibri, arial;\">" +
                        comment.Author + " on <a href=\"" + absoluteUrl + post.Url + "\">" + post.Title + "</a>:<br /><br />" +
                        comment.Content + "<br /><br />" +
                        (Blog.ModerateComments ? "<a href=\"" + approveUrl + "\">Approve comment</a> | " : string.Empty) +
                        "<a href=\"" + deleteUrl + "\">Delete comment</a>" +
                        "<br /><br /><hr />" +
                        "Website: " + comment.Website + "<br />" +
                        "E-mail: " + comment.Email + "<br />" +
                        "IP-address: " + comment.Ip +
                    "</div>";
        return mail;
    }

    private static void Validate(string name, string email, string content)
    {
        bool isName = !string.IsNullOrEmpty(name);
        bool isMail = !string.IsNullOrEmpty(email) && Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        bool isContent = !string.IsNullOrEmpty(content);

        if (!isName || !isMail || !isContent)
        {
            if (!isName)
                HttpContext.Current.Response.Status = "403 Please enter a valid name";
            else if (!isMail)
                HttpContext.Current.Response.Status = "403 Please enter a valid e-mail address";
            else if (!isContent)
                HttpContext.Current.Response.Status = "403 Please enter a valid comment";

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
        {
            FormsAuthentication.RedirectToLoginPage();
            context.Response.End();
        }

        Comment comment = GetComment(context, post);

        post.Comments.Remove(comment);
        Storage.Save(post);

        RedirectOnGET(context, post);
    }

    private static void Approve(HttpContext context, Post post)
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            FormsAuthentication.RedirectToLoginPage();
            context.Response.End();
        }

        Comment comment = GetComment(context, post);

        comment.IsApproved = true;
        Storage.Save(post);

        RedirectOnGET(context, post);
    }

    private static Comment GetComment(HttpContext context, Post post)
    {
        string commentId = context.Request["commentId"];
        Comment comment = post.Comments.FirstOrDefault(c => c.ID == commentId);

        if (comment == null)
            throw new HttpException(404, "Comment could not be found");

        return comment;
    }

    private static void RedirectOnGET(HttpContext context, Post post)
    {
        if (context.Request.HttpMethod == "GET")
            context.Response.Redirect(post.AbsoluteUrl.ToString() + "#comments", true);
    }

    public bool IsReusable
    {
        get { return false; }
    }
}