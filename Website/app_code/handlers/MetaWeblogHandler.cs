using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Security;

public interface IMetaWeblog
{
    #region MetaWeblog API

    [XmlRpcMethod("metaWeblog.newPost")]
    string AddPost(string blogid, string username, string password, Post post, bool publish);

    [XmlRpcMethod("metaWeblog.editPost")]
    bool UpdatePost(string postid, string username, string password, Post post, bool publish);

    [XmlRpcMethod("metaWeblog.getPost")]
    object GetPost(string postid, string username, string password);

    [XmlRpcMethod("metaWeblog.getCategories")]
    object[] GetCategories(string blogid, string username, string password);

    [XmlRpcMethod("metaWeblog.getRecentPosts")]
    object[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts);

    [XmlRpcMethod("metaWeblog.newMediaObject")]
    object NewMediaObject(string blogid, string username, string password, MediaObject mediaObject);

    #endregion

    #region Blogger API

    [XmlRpcMethod("blogger.deletePost")]
    [return: XmlRpcReturnValue(Description = "Returns true.")]
    bool DeletePost(string key, string postid, string username, string password, bool publish);

    [XmlRpcMethod("blogger.getUsersBlogs")]
    object[] GetUsersBlogs(string key, string username, string password);

    #endregion
}

public class MetaWeblogHandler : XmlRpcService, IMetaWeblog
{
    string IMetaWeblog.AddPost(string blogid, string username, string password, Post post, bool publish)
    {
        ValidateUser(username, password);

        if (!string.IsNullOrWhiteSpace(post.Slug))
        {
            post.Slug = PostHandler.CreateSlug(post.Slug);
        }
        else
        {
            post.Slug = PostHandler.CreateSlug(post.Title);    
        }
        
        post.IsPublished = publish;
        Storage.Save(post);

        return post.ID;
    }

    bool IMetaWeblog.UpdatePost(string postid, string username, string password, Post post, bool publish)
    {
        ValidateUser(username, password);

        Post match = Storage.GetAllPosts().FirstOrDefault(p => p.ID == postid);

        if (match != null)
        {
            match.Title = post.Title;
            match.Excerpt = post.Excerpt;
            match.Content = post.Content;

            if (!string.Equals(match.Slug, post.Slug, StringComparison.OrdinalIgnoreCase))
                match.Slug = PostHandler.CreateSlug(post.Slug);

            match.Categories = post.Categories;
            match.IsPublished = publish;

            Storage.Save(match);
        }

        return match != null;
    }

    bool IMetaWeblog.DeletePost(string key, string postid, string username, string password, bool publish)
    {
        ValidateUser(username, password);

        Post post = Storage.GetAllPosts().FirstOrDefault(p => p.ID == postid);

        if (post != null)
        {
            Storage.Delete(post);
        }

        return post != null;
    }

    object IMetaWeblog.GetPost(string postid, string username, string password)
    {
        ValidateUser(username, password);

        Post post = Storage.GetAllPosts().FirstOrDefault(p => p.ID == postid);

        if (post == null)
            throw new XmlRpcFaultException(0, "Post does not exist");

        return new
        {
            description = post.Content,
            title = post.Title,
            dateCreated = post.PubDate,
            wp_slug = post.Slug,
            categories = post.Categories.ToArray(),
            postid = post.ID,
            mt_excerpt = post.Excerpt ?? ""
        };
    }

    object[] IMetaWeblog.GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
    {
        ValidateUser(username, password);

        List<object> list = new List<object>();

        foreach (var post in Storage.GetAllPosts().Take(numberOfPosts))
        {
            var info = new
            {
                description = post.Content,
                title = post.Title,
                dateCreated = post.PubDate,
                wp_slug = post.Slug,
                postid = post.ID
            };

            list.Add(info);
        }

        return list.ToArray();
    }

    object[] IMetaWeblog.GetCategories(string blogid, string username, string password)
    {
        ValidateUser(username, password);

        var categories = Blog.GetCategories();

        var list = new List<object>();

        foreach ( string category in categories.Keys )
        {
            list.Add(new { title = category });
        }

        return list.ToArray();
    }

    object IMetaWeblog.NewMediaObject(string blogid, string username, string password, MediaObject media)
    {
        ValidateUser(username, password);

        string path = Blog.SaveFileToDisk(media.bits, Path.GetExtension(media.name));

        return new { url = path };
    }

    object[] IMetaWeblog.GetUsersBlogs(string key, string username, string password)
    {
        ValidateUser(username, password);

        return new[] 
        { 
            new 
            {
                blogid = "1",
                blogName = ConfigurationManager.AppSettings.Get("blog:name"),
                url = Context.Request.Url.Scheme + "://" + Context.Request.Url.Authority
            }
        };
    }

    private void ValidateUser(string username, string password)
    {
        if (!FormsAuthentication.Authenticate(username, password))
        {
            throw new XmlRpcFaultException(0, "User is not valid!");
        }
    }
}

[XmlRpcMissingMapping(MappingAction.Ignore)]
public struct MediaObject
{
    public string name;
    public string type;
    public byte[] bits;
}
