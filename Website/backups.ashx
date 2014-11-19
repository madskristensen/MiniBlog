<%@ WebHandler Language="C#" Class="backups" %>

using System;
using System.IO;
using System.Linq;
using System.Web;

public class backups : IHttpHandler {

    public void ProcessRequest(HttpContext context)
    {
        if (!context.User.Identity.IsAuthenticated)
            throw new HttpException(403, "No access");

        context.Response.ContentType = "text/html";
        foreach (FileInfo fi in new DirectoryInfo(HttpContext.Current.Server.MapPath("~/posts_backups/")).GetFiles().OrderByDescending(o => o.CreationTimeUtc))
        {
            context.Response.Write(fi.CreationTimeUtc.ToString("dd MMM yyyy HH:mm:ss") + "&nbsp;&nbsp;&nbsp;<a href='/Files/url_backups/" + fi.Name + "'>" + fi.Name + "</a><br />");
        }
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}