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
    private static string _theme = ConfigurationManager.AppSettings.Get("blog:theme");
    private static string _title = ConfigurationManager.AppSettings.Get("blog:name");
    private static int _postsPerPage = int.Parse(ConfigurationManager.AppSettings.Get("blog:postsPerPage"));
    private static int _commentDays = int.Parse(ConfigurationManager.AppSettings.Get("blog:daysToComment"));

    public static string Title
    {
        get { return _title; }
    }

    public static string Theme
    {
        get { return _theme; }
    }

    public static int PostsPerPage
    {
        get { return _postsPerPage; }
    }

    public static int DaysToComment
    {
        get { return _commentDays; }
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
                var post = Storage.GetAllPosts().FirstOrDefault(p => p.Slug == CurrentSlug);

                if (post != null && (post.IsPublished || HttpContext.Current.User.Identity.IsAuthenticated))
                    HttpContext.Current.Items["currentpost"] = Storage.GetAllPosts().FirstOrDefault(p => p.Slug == CurrentSlug);
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
        var posts = from p in Storage.GetAllPosts()
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
            string relative = VirtualPathUtility.ToAbsolute("~" + rootRelativePath);
            string absolute = HostingEnvironment.MapPath(relative);            

            if (!File.Exists(absolute))
            {
                throw new FileNotFoundException("File not found", absolute);
            }

            DateTime date = File.GetLastWriteTime(absolute);
            int index = relative.LastIndexOf('/');

            string result = relative.Insert(index, "/v-" + date.Ticks);

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