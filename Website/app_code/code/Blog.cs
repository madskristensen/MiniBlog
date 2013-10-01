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

    public static int PostsPerPage
    {
        get { return int.Parse(ConfigurationManager.AppSettings.Get("blog:postsPerPage")); }
    }

    public static string CurrentSlug
    {
        get { return (HttpContext.Current.Request.QueryString["slug"] ?? string.Empty).Trim().ToLowerInvariant(); }
    }

    public static string CurrentCategory
    {
        get { return (HttpContext.Current.Request.QueryString["category"] ?? string.Empty).Trim().ToLowerInvariant(); }
    }

    public static bool IsNewPost
    {
        get { return HttpContext.Current.Request.RawUrl.Trim('/') == "post/new"; }
    }

    public static Post CurrentPost
    {
        get
        {
            if (HttpContext.Current.Items["currentpost"] == null && !string.IsNullOrEmpty(CurrentSlug))
            {
                var post = Post.GetAllPosts().FirstOrDefault(p => p.Slug == CurrentSlug);

                if (post != null && (post.IsPublished || HttpContext.Current.User.Identity.IsAuthenticated))
                    HttpContext.Current.Items["currentpost"] = Post.GetAllPosts().FirstOrDefault(p => p.Slug == CurrentSlug);
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
        var posts = from p in Post.GetAllPosts()
                    where p.IsPublished || HttpContext.Current.User.Identity.IsAuthenticated
                    select p;

        string category = HttpContext.Current.Request.QueryString["category"];

        if (!string.IsNullOrEmpty(category))
        {
            posts = posts.Where(p => p.Categories.Any(c => string.Equals(c, category, StringComparison.OrdinalIgnoreCase)));
        }

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

    public static string FingerPrint(string rootRelativePath, string cdnPath = "")
    {
        if (!string.IsNullOrEmpty(cdnPath) && !HttpContext.Current.IsDebuggingEnabled)
        {
            return cdnPath;
        }

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

    public static void SetConditionalGetHeaders(DateTime lastModified, HttpContextBase context)
    {
        HttpResponseBase response = context.Response;
        HttpRequestBase request = context.Request;
        lastModified = new DateTime(lastModified.Year, lastModified.Month, lastModified.Day, lastModified.Hour, lastModified.Minute, lastModified.Second);

        string incomingDate = request.Headers["If-Modified-Since"];

        response.Cache.SetLastModified(lastModified);

        DateTime testDate = DateTime.MinValue;

        if (DateTime.TryParse(incomingDate, out testDate) && testDate == lastModified)
        {
            response.ClearContent();
            response.StatusCode = (int)System.Net.HttpStatusCode.NotModified;
            response.SuppressContent = true;
        }
    }
}