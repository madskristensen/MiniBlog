using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Hosting;

[XmlRpcMissingMapping(MappingAction.Ignore)]
public class Post
{
    public static List<Post> Posts = XmlStorage.Posts;
    private static XmlStorage _storage = new XmlStorage();

    public Post()
    {
        ID = Guid.NewGuid().ToString();
        Title = "My new post";
        Content = "the content";
        PubDate = DateTime.UtcNow;
        Categories = new string[0];
        Comments = new List<Comment>();
        IsPublished = true;
    }

    [XmlRpcMember("postid")]
    public string ID { get; set; }

    [XmlRpcMember("title")]
    public string Title { get; set; }

    [XmlRpcMember("wp_slug")]
    public string Slug { get; set; }

    [XmlRpcMember("description")]
    public string Content { get; set; }

    [XmlRpcMember("dateCreated")]
    public DateTime PubDate { get; set; }

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
        get { return new Uri(VirtualPathUtility.ToAbsolute("~/post/" + Slug), UriKind.Relative); }
    }

    public void Save()
    {
        _storage.Save(this);
    }

    public void Delete()
    {
        _storage.Delete(this);
    }
}