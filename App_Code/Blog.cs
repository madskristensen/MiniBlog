using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

public class Blog
{
    public static string Title
    {
        get { return ConfigurationManager.AppSettings.Get("blog:name"); }
    }

    public static string Theme
    {
        get { return ConfigurationManager.AppSettings.Get("blog:theme"); }
    }

    public static int CurrentPage(HttpRequestBase request)
    {
        int page = 0;
        if (int.TryParse(request.QueryString["page"], out page))
            return page;

        return 1;
    }

    public static string InsertLink(string rootRelativePath)
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