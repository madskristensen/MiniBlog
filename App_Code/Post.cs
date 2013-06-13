using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;

[XmlRpcMissingMapping(MappingAction.Ignore)]
public class Post
{
    private static string _folder = HostingEnvironment.MapPath("~/posts/");
    public static List<Post> Posts;

    static Post()
    {
        Posts = new List<Post>(LoadPosts());
        Posts = Posts.OrderByDescending(p => p.PubDate).ToList();
    }

    public Post()
    {
        ID = Guid.NewGuid().ToString();
        Title = "My new post";
        Content = "the content";
        PubDate = DateTime.UtcNow;
        Comments = new List<Comment>();
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

    public List<Comment> Comments { get; set; }

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
        string file = Path.Combine(_folder, ID + ".xml");

        XDocument doc = new XDocument(
                        new XElement("post",
                            new XElement("title", Title),
                            new XElement("slug", Slug),
                            new XElement("pubDate", PubDate.ToString("yyyy-MM-dd HH:mm:ss")),
                            new XElement("content", Content),
                            new XElement("ispublished", IsPublished),
                            new XElement("comments", string.Empty)
                        ));

        XElement comments = doc.XPathSelectElement("post/comments");

        foreach (Comment comment in Comments)
        {
            comments.Add(
                new XElement("comment",
                    new XElement("author", comment.Author),
                    new XElement("email", comment.Email),
                    new XElement("website", comment.Website),
                    new XElement("ip", comment.Ip),
                    new XElement("userAgent", comment.UserAgent),
                    new XElement("date", comment.PubDate.ToString("yyyy-MM-dd HH:m:ss")),
                    new XElement("content", comment.Content),
                    new XElement("isAdmin", comment.IsAdmin),
                    new XAttribute("id", comment.ID)
                ));
        }

        if (!File.Exists(file)) // New post
            Post.Posts.Insert(0, this);

        doc.Save(file);
    }

    public void Delete()
    {
        string file = Path.Combine(_folder, ID + ".xml");
        File.Delete(file);
        Post.Posts.Remove(this);
    }

    private static IEnumerable<Post> LoadPosts()
    {
        foreach (string file in Directory.GetFiles(_folder, "*.xml", SearchOption.TopDirectoryOnly))
        {
            XElement doc = XElement.Load(file);

            Post post = new Post()
            {
                ID = Path.GetFileNameWithoutExtension(file),
                Title = ReadValue(doc, "title"),
                Content = ReadValue(doc, "content"),
                Slug = ReadValue(doc, "slug").ToLowerInvariant(),
                PubDate = DateTime.Parse(ReadValue(doc,"pubDate")),
                IsPublished = bool.Parse(ReadValue(doc, "ispublished", "true")),
            };

            LoadComments(post, doc);

            yield return post;
        }
    }

    private static void LoadComments(Post post, XElement doc)
    {
        var comments = doc.Element("comments");

        if (comments == null)
            return;

        foreach (var node in comments.Elements("comment"))
        {
            Comment comment = new Comment()
            {
                ID = node.Attribute("id").Value,
                Author = ReadValue(node, "author"),
                Email = ReadValue(node, "email"),
                Website = ReadValue(node, "website"),
                Ip = ReadValue(node, "ip"),
                UserAgent = ReadValue(node, "userAgent"),
                IsAdmin =  bool.Parse(ReadValue(node, "isAdmin", "false")),
                Content = ReadValue(node, "content").Replace("\n", "<br />"),
                PubDate = DateTime.Parse(ReadValue(node, "date", "2000-01-01")),
            };

            post.Comments.Add(comment);
        }
    }

    private static string ReadValue(XElement doc, XName name, string defaultValue = "")
    {
        if (doc.Element(name) != null)
            return doc.Element(name).Value;

        return defaultValue;
    }
}