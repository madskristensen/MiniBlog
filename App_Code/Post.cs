using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

public class Post
{
    public static List<Post> Posts = new List<Post>();
    private static string _folder = HostingEnvironment.MapPath("~/App_Data/posts/");

    static Post()
    {
        foreach (string file in Directory.GetFiles(HostingEnvironment.MapPath("~/App_Data/posts/"), "*.xml"))
        {
            XElement doc = XElement.Load(file);

            Post post = new Post()
            {
                ID = Path.GetFileNameWithoutExtension(file),
                Title = doc.Element("title").Value,
                Slug = doc.Element("slug").Value.Replace(" ", "-").ToLowerInvariant(),
                Content = doc.Element("content").Value,
                PubDate = DateTime.Parse(doc.Element("pubDate").Value),
                LastModified = DateTime.Parse(doc.Element("lastModified").Value),
            };

            Posts.Add(post);
        }

        Posts = Posts.OrderByDescending(p => p.PubDate).ToList();
    }

    public Post()
    {
        ID = Guid.NewGuid().ToString();
        Title = "My new post";
        Content = "the content";
        PubDate = DateTime.UtcNow;
        LastModified = DateTime.UtcNow;
    }

    public string ID { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public DateTime PubDate { get; set; }
    public DateTime LastModified { get; set; }

    public void Save()
    {
        string file = Path.Combine(_folder, ID + ".xml");
        XDocument doc;

        if (File.Exists(file))
        {
            doc = XDocument.Load(file);
            XElement root = doc.Element("post");
            root.Element("slug").SetValue(Slug);
            root.Element("title").SetValue(Title);
            root.Element("content").SetValue(Content);
            root.Element("lastModified").SetValue(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
        }
        else
        {
            doc = new XDocument(
                new XElement("post",
                    new XElement("title", Title),
                    new XElement("slug", Slug),
                    new XElement("content", Content),
                    new XElement("pubDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm")),
                    new XElement("lastModified", DateTime.Now.ToString("yyyy-MM-dd HH:mm"))
                )
            );

            Post.Posts.Insert(0, this);
        }

        doc.Save(file);
    }

    public void Delete()
    {
        string file = Path.Combine(_folder, ID + ".xml");
        File.Delete(file);
        Post.Posts.Remove(this);
    }

    public static IEnumerable<Post> GetPosts(int postsPerPage)
    {
        HttpRequest request = HttpContext.Current.Request;
        string slug = request.QueryString["slug"];

        if (!string.IsNullOrEmpty(slug))
        {
            var post = Posts.FirstOrDefault(p => p.Slug.Equals(slug.Trim(), StringComparison.OrdinalIgnoreCase));

            if (post == null)
                throw new HttpException(404, "Blog post does not exist");

            return new[] { post };
        }

        int page = Blog.CurrentPage(new HttpRequestWrapper(request));

        return Posts.Skip(postsPerPage * (page - 1)).Take(postsPerPage);
    }
}