using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Web;

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
}