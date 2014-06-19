using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;
using System.Xml.XPath;

public static class Storage
{
    private static string _folder = HostingEnvironment.MapPath("~/posts/");

    public static List<Post> GetAllPosts()
    {
        if (HttpRuntime.Cache["posts"] == null)
            LoadPosts();

        if (HttpRuntime.Cache["posts"] != null)
        {
            return (List<Post>)HttpRuntime.Cache["posts"];
        }
        return new List<Post>();
    }

    // Can this be done async?
    public static void Save(Post post)
    {
        string file = Path.Combine(_folder, post.ID + ".xml");
        post.LastModified = DateTime.UtcNow;

        XDocument doc = new XDocument(
                        new XElement("post",
                            new XElement("title", post.Title),
                            new XElement("slug", post.Slug),
                            new XElement("author", post.Author),
                            new XElement("pubDate", post.PubDate.ToString("yyyy-MM-dd HH:mm:ss")),
                            new XElement("lastModified", post.LastModified.ToString("yyyy-MM-dd HH:mm:ss")),
                            new XElement("excerpt", post.Excerpt),
                            new XElement("content", post.Content),
                            new XElement("ispublished", post.IsPublished),
                            new XElement("categories", string.Empty),
                            new XElement("comments", string.Empty)
                        ));

        XElement categories = doc.XPathSelectElement("post/categories");
        foreach (string category in post.Categories)
        {
            categories.Add(new XElement("category", category));
        }

        XElement comments = doc.XPathSelectElement("post/comments");
        foreach (Comment comment in post.Comments)
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
                    new XAttribute("isAdmin", comment.IsAdmin),
                    new XAttribute("isApproved", comment.IsApproved),
                    new XAttribute("id", comment.ID)
                ));
        }

        if (!File.Exists(file)) // New post
        {
            var posts = GetAllPosts();
            posts.Insert(0, post);
            posts.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
            HttpRuntime.Cache.Insert("posts", posts);
        }
        else
        {
            Blog.ClearStartPageCache();
        }

        doc.Save(file);
    }

    public static void Delete(Post post)
    {
        var posts = GetAllPosts();
        string file = Path.Combine(_folder, post.ID + ".xml");
        File.Delete(file);
        posts.Remove(post);
        Blog.ClearStartPageCache();
    }

    private static void LoadPosts()
    {
        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        List<Post> list = new List<Post>();

        // Can this be done in parallel to speed it up?
        foreach (string file in Directory.EnumerateFiles(_folder, "*.xml", SearchOption.TopDirectoryOnly))
        {
            XElement doc = XElement.Load(file);

            Post post = new Post()
            {
                ID = Path.GetFileNameWithoutExtension(file),
                Title = ReadValue(doc, "title"),
                Author = ReadValue(doc, "author"),
                Excerpt = ReadValue(doc, "excerpt"),
                Content = ReadValue(doc, "content"),
                Slug = ReadValue(doc, "slug").ToLowerInvariant(),
                PubDate = DateTime.Parse(ReadValue(doc, "pubDate")),
                LastModified = DateTime.Parse(ReadValue(doc, "lastModified", DateTime.Now.ToString())),
                IsPublished = bool.Parse(ReadValue(doc, "ispublished", "true")),
            };

            LoadCategories(post, doc);
            LoadComments(post, doc);
            list.Add(post);
        }

        if (list.Count > 0)
        {
            list.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
            HttpRuntime.Cache.Insert("posts", list);
        }
    }

    private static void LoadCategories(Post post, XElement doc)
    {
        XElement categories = doc.Element("categories");
        if (categories == null)
            return;

        List<string> list = new List<string>();

        foreach (var node in categories.Elements("category"))
        {
            list.Add(node.Value);
        }

        post.Categories = list.ToArray();
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
                ID = ReadAttribute(node, "id"),
                Author = ReadValue(node, "author"),
                Email = ReadValue(node, "email"),
                Website = ReadValue(node, "website"),
                Ip = ReadValue(node, "ip"),
                UserAgent = ReadValue(node, "userAgent"),
                IsAdmin = bool.Parse(ReadAttribute(node, "isAdmin", "false")),
                IsApproved = bool.Parse(ReadAttribute(node, "isApproved", "true")),
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

    private static string ReadAttribute(XElement element, XName name, string defaultValue = "")
    {
        if (element.Attribute(name) != null)
            return element.Attribute(name).Value;

        return defaultValue;
    }
}