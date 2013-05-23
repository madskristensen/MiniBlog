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
    private static string _folder = HostingEnvironment.MapPath("~/Data/posts/");

    static Post()
    {
        foreach (string file in Directory.GetFiles(_folder, "*.xml", SearchOption.AllDirectories))
        {
            XElement doc = XElement.Load(file);

            Post post = new Post()
            {
                ID = Path.GetFileNameWithoutExtension(file),
                Title = doc.Element("title").Value,
                Slug = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(file)),
                Content = doc.Element("content").Value,
                PubDate = File.GetCreationTimeUtc(file),
                LastModified = File.GetLastWriteTimeUtc(file),
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
    }

    public string ID { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public DateTime PubDate { get; set; }
    public DateTime LastModified { get; set; }

    public void Save()
    {
        string file = Path.Combine(_folder, Slug, ID + ".xml");
        XDocument doc;

        if (File.Exists(file))
        {
            doc = XDocument.Load(file);
            XElement root = doc.Element("post");
            root.Element("title").SetValue(Title);
            root.Element("content").SetValue(Content);
        }
        else
        {
            doc = new XDocument(
                new XElement("post",
                    new XElement("title", Title),
                    new XElement("content", Content)
                )
            );

            Post.Posts.Insert(0, this);
        }

        string directory = Path.GetDirectoryName(file);

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        doc.Save(file);
    }

    public void Delete()
    {
        string directory = Path.Combine(_folder, Slug);
        Directory.Delete(directory, true);
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