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

        List<SyndicationItem> items = new List<SyndicationItem>();

        foreach (Post post in Post.Posts.Take(10))
        {
            var item = new SyndicationItem(post.Title, post.Content, post.AbsoluteUrl, post.ID, post.PubDate);
            items.Add(item);
        }

        var feed = new SyndicationFeed(items)
        {
            Title = new TextSyndicationContent(Blog.Title),
            Description = new TextSyndicationContent("Latest blog posts"),
            BaseUri = new Uri(context.Request.Url.Scheme + "://" + context.Request.Url.Authority)
        };

        using (var writer = new XmlTextWriter(context.Response.Output))
        {
            var formatter = new Rss20FeedFormatter(feed);
            formatter.WriteTo(writer);
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}