using Microsoft.Ajax.Utilities;
using System;
using System.IO;
using System.Web;
using System.Web.Caching;

public class MinifyHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string file = context.Server.MapPath(context.Request.CurrentExecutionFilePath);
        string extension = Path.GetExtension(file);

        if (context.IsDebuggingEnabled)
        {
            context.Response.TransmitFile(file);
        }
        else
        {
            Minify(context.Response, file, extension);
        }

        SetHeaders(context.Response, file, extension);
    }

    private static void SetHeaders(HttpResponse response, string file, string extension)
    {
        if (extension == ".css")
        {
            response.ContentType = "text/css";
        }
        else if (extension == ".js")
        {
            response.ContentType = "text/javascript";
        }

        response.Cache.SetLastModified(File.GetLastWriteTimeUtc(file));
        response.Cache.SetValidUntilExpires(true);
        response.Cache.SetExpires(DateTime.Now.AddYears(1));
        response.Cache.SetCacheability(HttpCacheability.Public);
        response.AddCacheDependency(new CacheDependency(file));
    }

    private static void Minify(HttpResponse response, string file, string extension)
    {
        string content = File.ReadAllText(file);
        Minifier minifier = new Minifier();

        if (extension == ".css")
        {
            CssSettings settings = new CssSettings() { CommentMode = CssComment.None };
            response.Write(minifier.MinifyStyleSheet(content, settings));
        }
        else if (extension == ".js")
        {
            CodeSettings settings = new CodeSettings() { PreserveImportantComments = false };
            response.Write(minifier.MinifyJavaScript(content, settings));
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}