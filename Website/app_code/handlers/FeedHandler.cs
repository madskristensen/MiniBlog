using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;

public class FeedHandler : IHttpHandler
{
	public void ProcessRequest(HttpContext context)
	{
		bool getAllItems = context.Request.Url.PathAndQuery.Contains("tapir");
		SyndicationFeed feed = new SyndicationFeed()
		{
			Title = new TextSyndicationContent(Blog.Title),
			Description = new TextSyndicationContent("Latest blog posts"),
			BaseUri = new Uri(context.Request.Url.Scheme + "://" + context.Request.Url.Authority),
			Items = GetItems(getAllItems),
		};

		feed.Links.Add(new SyndicationLink(feed.BaseUri));

		using (var writer = new XmlTextWriter(context.Response.Output))
		{
			var formatter = GetFormatter(context, feed);
			formatter.WriteTo(writer);
		}

		context.Response.ContentType = "text/xml";
	}

	private IEnumerable<SyndicationItem> GetItems(bool getAllItems)
	{
		int numberOfItems = getAllItems ? 0 : 20;
		foreach (Post p in Blog.GetPosts(numberOfItems))
		{
			var item = new SyndicationItem(p.Title, p.Content, p.AbsoluteUrl, p.AbsoluteUrl.ToString(), p.LastModified);
			item.Authors.Add(new SyndicationPerson("", p.Author, ""));
			yield return item;
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