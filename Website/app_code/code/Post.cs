using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Web;

[XmlRpcMissingMapping(MappingAction.Ignore)]
public class Post
{
    private static IBlogStorage _storage;

    static Post()
    {
        AssemblyCatalog catalog = new AssemblyCatalog(typeof(Post).Assembly);
        CompositionContainer container = new CompositionContainer(catalog);
        _storage = container.GetExportedValues<IBlogStorage>().ElementAt(0);
    }

    public Post()
    {
        ID = Guid.NewGuid().ToString();
        Title = "My new post";
        Author = HttpContext.Current.User.Identity.Name;
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

    [XmlRpcMember("author")]
    public string Author { get; set; }

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

    public static List<Post> GetAllPosts()
    {
        return _storage.GetAllPosts();
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