using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Linq;

public class Blog
{
    public static List<Post> Posts = new List<Post>();

    static Blog()
    {
        foreach (string file in Directory.GetFiles(HostingEnvironment.MapPath("~/App_Data/"), "*.xml"))
        {
            XElement doc = XElement.Load(file);

            Post post = new Post()
            {
                Title = doc.Element("title").Value,
                Slug = doc.Element("slug").Value,
                Content = doc.Element("content").Value,
                PubDate = DateTime.Parse(doc.Element("pubDate").Value)
            };

            Posts.Add(post);
        }

        Posts = Posts.OrderByDescending(p => p.PubDate).ToList();
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

        int page = 0;
        int.TryParse(request.QueryString["page"], out page);

        return Posts.Skip(postsPerPage * (page - 1)).Take(postsPerPage);
    }
}

public static class ExtensionMethods
{
    public static int CurrentPage(this HttpRequestBase request)
    {
        int page = 0;
        if (int.TryParse(request.QueryString["page"], out page))
            return page;

        return 1;
    }
}

public class Post
{
    public string Title { get; set; }
    public string Slug { get; set; }
    public string Content { get; set; }
    public DateTime PubDate { get; set; }
}