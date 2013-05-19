using System.Configuration;
using System.Web;

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
}