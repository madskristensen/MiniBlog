using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

public static class Blog
{
    public static string Title
    {
        get { return ConfigurationManager.AppSettings.Get("blog:name"); }
    }

    public static string Theme
    {
        get { return ConfigurationManager.AppSettings.Get("blog:theme"); }
    }

    public static string CurrentSlug
    {
        get { return (HttpContext.Current.Request.QueryString["slug"] ?? string.Empty).Trim().ToLowerInvariant(); }
    }

    public static bool IsNewPost
    {
        get { return HttpContext.Current.Request.RawUrl.Trim('/') == "post/new"; }
    }

    public static Post CurrentPost
    {
        get
        {
            if (HttpContext.Current.Items["currentpost"] == null)
            {
                var post = Post.Posts.FirstOrDefault(p => p.Slug == CurrentSlug);

                if (post != null && (post.IsPublished || HttpContext.Current.User.Identity.IsAuthenticated))
                    HttpContext.Current.Items["currentpost"] = Post.Posts.FirstOrDefault(p => p.Slug == CurrentSlug);
            }

            return HttpContext.Current.Items["currentpost"] as Post;
        }
    }

    public static int CurrentPage
    {
        get
        {
            int page = 0;
            if (int.TryParse(HttpContext.Current.Request.QueryString["page"], out page))
                return page;

            return 1;
        }
    }

    public static IEnumerable<Post> GetPosts(int postsPerPage)
    {
        var posts = from p in Post.Posts
                    where p.IsPublished || HttpContext.Current.User.Identity.IsAuthenticated
                    select p;

        return posts.Skip(postsPerPage * (CurrentPage - 1)).Take(postsPerPage);
    }

    public static string SaveFileToDisk(byte[] bytes, string extension)
    {
        string relative = "~/posts/files/" + Guid.NewGuid() + "." + extension.Trim('.');
        string file = HostingEnvironment.MapPath(relative);

        File.WriteAllBytes(file, bytes);

        var cruncher = new ImageCruncher.Cruncher();
        cruncher.CrunchImages(file);

        return VirtualPathUtility.ToAbsolute(relative);
    }

    public static string FingerPrint(string rootRelativePath)
    {
        if (HttpRuntime.Cache[rootRelativePath] == null)
        {
            string absolute = HostingEnvironment.MapPath("~" + rootRelativePath);

            if (!File.Exists(absolute))
            {
                throw new FileNotFoundException("File not found", absolute);
            }

            DateTime date = File.GetLastWriteTime(absolute);
            int index = rootRelativePath.LastIndexOf('/');

            string result = rootRelativePath.Insert(index, "/v-" + date.Ticks);
            HttpRuntime.Cache.Insert(rootRelativePath, result, new CacheDependency(absolute));
        }

        return HttpRuntime.Cache[rootRelativePath] as string;
    }
}