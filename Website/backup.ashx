<%@ WebHandler Language="C#" Class="backup" %>

using System;
using System.IO.Compression;
using System.Web;

public class backup : IHttpHandler {

    public void ProcessRequest(HttpContext context)
    {
        if (!context.User.Identity.IsAuthenticated)
            throw new HttpException(403, "No access");

        string fileName = string.Format("miniblog_backup_-_{0}.zip", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
        string zipOutputLocation = HttpContext.Current.Server.MapPath("~/posts_backups/") + fileName;
        ZipFile.CreateFromDirectory(HttpContext.Current.Server.MapPath("~/posts/"), zipOutputLocation, CompressionLevel.Optimal, false);
        context.Response.Clear();
        context.Response.ContentType = "application/zip";
        context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
        context.Response.TransmitFile(zipOutputLocation);
        context.Response.Flush();
    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }

}