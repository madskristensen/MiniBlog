<%@ Application Language="C#" %>

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
    }
       
</script>
