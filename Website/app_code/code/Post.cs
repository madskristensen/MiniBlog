using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CookComputing.XmlRpc;

[XmlRpcMissingMapping(MappingAction.Ignore)]
public class Post
{
    public Post()
    {
        ID = Guid.NewGuid().ToString();
        Title = "My new post";
        Author = HttpContext.Current.User.Identity.Name;
        Content = "the content";
        PubDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
        Categories = new string[0];
        Comments = new List<Comment>();
        IsPublished = true;
    }

    [XmlRpcMember("postid")]
    public string ID { get; set; }

    [XmlRpcMember("title")]
    public string Title { get; set; }

    [XmlRpcMember("author")]
    public string Author { get; set; }

    [XmlRpcMember("wp_slug")]
    public string Slug { get; set; }

    [XmlRpcMember("mt_excerpt")]
    public string Excerpt { get; set; }

    [XmlRpcMember("description")]
    public string Content { get; set; }

    [XmlRpcMember("dateCreated")]
    public DateTime PubDate { get; set; }

    [XmlRpcMember("dateModified")]
    public DateTime LastModified { get; set; }

    public bool IsPublished { get; set; }

    [XmlRpcMember("categories")]
    public string[] Categories { get; set; }
    public List<Comment> Comments { get; private set; }

    public Uri AbsoluteUrl
    {
        get
        {
            Uri requestUrl = HttpContext.Current.Request.Url;
            return new Uri(requestUrl.Scheme + "://" + requestUrl.Authority + Url, UriKind.Absolute);
        }
    }

    public Uri Url
    {
        get
        {
            return new Uri(VirtualPathUtility.ToAbsolute("~/post/" + Slug), UriKind.Relative);
        }
    }

    public bool AreCommentsOpen(HttpContextBase context)
    {
        return PubDate > DateTime.UtcNow.AddDays(-Blog.DaysToComment) || context.User.Identity.IsAuthenticated;
    }

    public int CountApprovedComments(HttpContextBase context)
    {
        return (Blog.ModerateComments && !context.User.Identity.IsAuthenticated) ? this.Comments.Count(c => c.IsApproved) : this.Comments.Count;
    }

    public string GetHtmlContent()
    {
        string result = Content;

        // Youtube content embedded using this syntax: [youtube:xyzAbc123]
        var video = "<div class=\"video\"><iframe src=\"//www.youtube.com/embed/{0}?modestbranding=1&amp;theme=light\" allowfullscreen></iframe></div>";
        result = Regex.Replace(result, @"\[youtube:(.*?)\]", (Match m) => string.Format(video, m.Groups[1].Value));

        // Images replaced by CDN paths if they are located in the /posts/ folder
        var cdn = ConfigurationManager.AppSettings.Get("blog:cdnUrl");
        var root = ConfigurationManager.AppSettings.Get("blog:path") + "/posts/";

        if (!root.StartsWith("/"))
            root = "/" + root;

        result = Regex.Replace(result, "<img.*?src=\"([^\"]+)\"", (Match m) =>
        {
            string src = m.Groups[1].Value;
            int index = src.IndexOf(root);

            if (index > -1)
            {
                string clean = src.Substring(index);
                return m.Value.Replace(src, cdn + clean);
            }

            return m.Value;
        });

        return result;
    }
}