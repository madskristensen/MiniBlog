using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;

/// <summary>
/// Serves post content files
/// </summary>
public class ContentHandler : IHttpHandler
{

    public bool IsReusable
    {
        get { return false; }
    }

    public void ProcessRequest(HttpContext context)
    {
        var dir = HostingEnvironment.MapPath("~/App_Data/posts/files/");
        var fileName = context.Request.Url.LocalPath.Substring(13);
        var path = Path.Combine(dir, fileName);
        if (!File.Exists(path))
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            return;
        }

        context.Response.ContentType = GetMimeType(path);
        using (var fs = File.OpenRead(path))
        {
            context.Response.AddHeader("content-length", fs.Length.ToString());

            var buffer = new byte[2048];
            var count = fs.Read(buffer, 0, buffer.Length);
            while (count > 0)
            {
                context.Response.OutputStream.Write(buffer, 0, count);
                context.Response.OutputStream.Flush();
                count = fs.Read(buffer, 0, buffer.Length);
            }
        }
    }

    private string GetMimeType(string filePath)
    {
        switch (Path.GetExtension(filePath).ToLower())
        {
            case ".png":
                return "image/png";
        }
        return "application/octet-stream";
    }

}