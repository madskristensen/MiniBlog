using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

public class Post
{
    private static string _folder = HostingEnvironment.MapPath("~/posts/");

    static Post()
    {
        foreach (string file in Directory.GetFiles(_folder, "*.xml", SearchOption.AllDirectories))
        {
            XElement doc = XElement.Load(file);

            Post post = new Post()
            {
                ID = Path.GetFileNameWithoutExtension(file),
                Title = doc.Element("title").Value,
                Content = doc.Element("content").Value,
                Slug = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(file)),
                PubDate = File.GetCreationTimeUtc(file),
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

    public static List<Post> Posts = new List<Post>();
    public string ID { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public DateTime PubDate { get; set; }

    public void Save()
    {
        string file = Path.Combine(_folder, Slug, ID + ".xml");
        string directory = Path.GetDirectoryName(file);

        XDocument doc = doc = new XDocument(
                        new XElement("post",
                            new XElement("title", Title),
                            new XElement("content", Content)
                        ));


        if (!Directory.Exists(directory)) // New post
        {
            Directory.CreateDirectory(directory);
            Post.Posts.Insert(0, this);
        }

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