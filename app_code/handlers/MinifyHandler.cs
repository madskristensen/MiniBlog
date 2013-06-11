using Microsoft.Ajax.Utilities;
using System;
using System.IO;
using System.Web;

public class MinifyHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        string file = context.Server.MapPath(context.Request.CurrentExecutionFilePath);
        
        Minify(context, file);

        context.Response.Cache.SetLastModified(File.GetLastWriteTimeUtc(file));
        context.Response.Cache.SetValidUntilExpires(true);
        context.Response.Cache.SetExpires(DateTime.Now.AddYears(1));
        context.Response.Cache.SetCacheability(HttpCacheability.Public);
    }

    private static void Minify(HttpContext context, string file)
    {
        string extension = Path.GetExtension(file);
        string content = File.ReadAllText(file);
        Minifier minifier = new Minifier();

        if (extension == ".css")
        {
            CssSettings settings = new CssSettings();
            settings.CommentMode = CssComment.None;

            string result = minifier.MinifyStyleSheet(content, settings);
            context.Response.Write(result);
            context.Response.ContentType = "text/css";
        }
        else if (extension == ".js")
        {
            CodeSettings settings = new CodeSettings();
            settings.PreserveImportantComments = false;

            string result = minifier.MinifyJavaScript(content, settings);
            context.Response.Write(result);
            context.Response.ContentType = "text/javascript";
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}