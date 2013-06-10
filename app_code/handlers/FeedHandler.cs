using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;

public class FeedHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/rss+xml";

        SyndicationFeed feed = new SyndicationFeed()
        {
            Title = new TextSyndicationContent(Blog.Title),
            Description = new TextSyndicationContent("Latest blog posts"),
            BaseUri = new Uri(context.Request.Url.Scheme + "://" + context.Request.Url.Authority),
            Items = GetItems()
        };

        using (var writer = new XmlTextWriter(context.Response.Output))
        {
            var formatter = new Rss20FeedFormatter(feed);
            formatter.WriteTo(writer);
        }
    }

    private IEnumerable<SyndicationItem> GetItems()
    {
        foreach (Post p in Post.Posts.Take(10))
        {
            yield return new SyndicationItem(p.Title, p.Content, p.AbsoluteUrl, p.ID, p.PubDate);
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}