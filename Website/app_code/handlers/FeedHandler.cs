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
        SyndicationFeed feed = new SyndicationFeed()
        {
            Title = new TextSyndicationContent(Blog.Title),
            Description = new TextSyndicationContent("Latest blog posts"),
            BaseUri = new Uri(context.Request.Url.Scheme + "://" + context.Request.Url.Authority),
            Items = GetItems()
        };

        using (var writer = new XmlTextWriter(context.Response.Output))
        {
            var formatter = GetFormatter(context, feed);
            formatter.WriteTo(writer);
        }
    }

    private IEnumerable<SyndicationItem> GetItems()
    {
        foreach (Post p in Post.GetAllPosts().Take(10))
        {
            yield return new SyndicationItem(p.Title, p.Content, p.AbsoluteUrl, p.ID, p.PubDate);
        }
    }

    private SyndicationFeedFormatter GetFormatter(HttpContext context, SyndicationFeed feed)
    {
        string path = context.Request.Path.Trim('/');
        int index = path.LastIndexOf('/');

        if (index > -1 && path.Substring(index + 1) == "atom")
        {
            context.Response.ContentType = "application/atom+xml";
            return new Atom10FeedFormatter(feed);
        }

        context.Response.ContentType = "application/rss+xml";
        return new Rss20FeedFormatter(feed);
    }

    public bool IsReusable
    {
        get { return false; }
    }
}