<%@ Application Language="C#" %>
<%@ Import Namespace="System.IO" %>

<script RunAt="server">

    public override string GetVaryByCustomString(HttpContext context, string arg)
    {
        if (arg == "authenticated")
        {
            HttpCookie cookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (cookie != null)
                return cookie.Value;
        }

        return base.GetVaryByCustomString(context, arg);
    }

    public void Application_BeginRequest(object sender, EventArgs e)
    {
        Context.Items["IIS_WasUrlRewritten"] = "false";
        System.Web.WebPages.WebPageHttpHandler.DisableWebPagesResponseHeader = true;

        var application = sender as HttpApplication;
        if (application != null && application.Context != null)
        {
            application.Context.Response.Headers.Remove("Server");
        }
    }

    public void Application_Start(object sender, EventArgs e)
    {
        string path = HttpContext.Current.Server.MapPath("~/posts_backups/");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        path = HttpContext.Current.Server.MapPath("~/posts/");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public void Application_OnError()
    {
        var request = HttpContext.Current.Request;
        var exception = Server.GetLastError() as HttpException;
        if (exception == null) return;
        
        //Prevents customError behavior when the request is determined to be an AJAX request.
        if (request["X-Requested-With"] == "XMLHttpRequest" || request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            Server.ClearError();
            Response.ClearContent();
            Response.StatusCode = exception.GetHttpCode();
            Response.StatusDescription = exception.Message;
            Response.Write(string.Format("<html><body><h1>{0} {1}</h1></body></html>", exception.GetHttpCode(), exception.Message));
        }
    }
       
</script>
