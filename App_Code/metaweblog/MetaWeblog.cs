using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;

public interface IMetaWeblog
{
    #region MetaWeblog API

    [XmlRpcMethod("metaWeblog.newPost")]
    string AddPost(string blogid, string username, string password, Post post, bool publish);

    [XmlRpcMethod("metaWeblog.editPost")]
    bool UpdatePost(string postid, string username, string password, Post post, bool publish);

    [XmlRpcMethod("metaWeblog.getPost")]
    Post GetPost(string postid, string username, string password);

    //[XmlRpcMethod("metaWeblog.getCategories")]
    //CategoryInfo[] GetCategories(string blogid, string username, string password);

    [XmlRpcMethod("metaWeblog.getRecentPosts")]
    Post[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts);

    [XmlRpcMethod("metaWeblog.newMediaObject")]
    object NewMediaObject(string blogid, string username, string password,
        MediaObject mediaObject);

    #endregion

    #region Blogger API

    [XmlRpcMethod("blogger.deletePost")]
    [return: XmlRpcReturnValue(Description = "Returns true.")]
    bool DeletePost(string key, string postid, string username, string password, bool publish);

    [XmlRpcMethod("blogger.getUsersBlogs")]
    object[] GetUsersBlogs(string key, string username, string password);

    //[XmlRpcMethod("blogger.getUserInfo")]
    //UserInfo GetUserInfo(string key, string username, string password);

    #endregion
}

public class MetaWeblog : XmlRpcService, IMetaWeblog
{
    #region IMetaWeblog Members

    string IMetaWeblog.AddPost(string blogid, string username, string password, Post post, bool publish)
    {
        if (ValidateUser(username, password))
        {
            //Post newPost = new Post()
            //{
            //    Content = post.description,
            //    Title = post.title,
            //    PubDate = post.dateCreated > DateTime.MinValue ? post.dateCreated.ToUniversalTime() : DateTime.UtcNow,
            //    Slug = PostHandler.CreateSlug(post.title),
            //};

            post.Save();

            return post.ID;
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    bool IMetaWeblog.UpdatePost(string postid, string username, string password, Post post, bool publish)
    {
        if (ValidateUser(username, password))
        {
            if (Post.Posts.Exists(p => p.ID == postid))
            {
                post.Save();
                return true;
            }

            return false;
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    bool IMetaWeblog.DeletePost(string key, string postid, string username, string password, bool publish)
    {
        if (ValidateUser(username, password))
        {
            Post post = Post.Posts.FirstOrDefault(p => p.ID == postid);

            if (post != null)
            {
                post.Delete();
                return true;
            }

            return false;
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    Post IMetaWeblog.GetPost(string postid, string username, string password)
    {
        if (ValidateUser(username, password))
        {

            return Post.Posts.FirstOrDefault(p => p.ID == postid);

            //if (newPost != null)
            //{
            //    PostInfo post = new PostInfo()
            //    {
            //        title = newPost.Title,
            //        description = newPost.Content,
            //        wp_slug = newPost.Slug,
            //        dateCreated = newPost.PubDate,
            //        postid = newPost.ID,
            //    };

            //    return post;
            //}
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    //CategoryInfo[] IMetaWeblog.GetCategories(string blogid, string username, string password)
    //{
    //    if (ValidateUser(username, password))
    //    {
    //        List<CategoryInfo> categoryInfos = new List<CategoryInfo>();

    //        // TODO: Implement your own logic to get category info and set the categoryInfos

    //        return categoryInfos.ToArray();
    //    }
    //    throw new XmlRpcFaultException(0, "User is not valid!");
    //}

    Post[] IMetaWeblog.GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
    {
        if (ValidateUser(username, password))
        {
            //List<PostInfo> list = new List<PostInfo>();

            return Post.Posts.Take(numberOfPosts).ToArray();
            //foreach (var post in posts)
            //{
            //    PostInfo info = new PostInfo()
            //    {
            //        description = post.Content,
            //        title = post.Title,
            //        dateCreated = post.PubDate,
            //        wp_slug = post.Slug,
            //        postid = post.ID
            //    };

            //    list.Add(info);
            //}

            //return list.ToArray();
        }
        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    object IMetaWeblog.NewMediaObject(string blogid, string username, string password, MediaObject mediaObject)
    {
        if (ValidateUser(username, password))
        {
            string folder = Context.Server.MapPath("~/posts/files/");
            string fileName = Guid.NewGuid() + Path.GetExtension(mediaObject.name);
            string path = Path.Combine(folder, fileName);

            File.WriteAllBytes(path, mediaObject.bits);

            return new { url = VirtualPathUtility.ToAbsolute("~/posts/files/" + fileName) };
        }
        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    object[] IMetaWeblog.GetUsersBlogs(string key, string username, string password)
    {
        if (ValidateUser(username, password))
        {
            return new[] { 
                new {
                    blogid = "1",
                    blogName = ConfigurationManager.AppSettings.Get("blog:name"),
                    url = Context.Request.Url.Scheme + "://" + Context.Request.Url.Authority
                }
            };
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    //UserInfo IMetaWeblog.GetUserInfo(string key, string username, string password)
    //{
    //    if (ValidateUser(username, password))
    //    {
    //        UserInfo info = new UserInfo();

    //        // TODO: Implement your own logic to get user info objects and set the info

    //        return info;
    //    }
    //    throw new XmlRpcFaultException(0, "User is not valid!");
    //}

    #endregion

    #region Private Methods

    private bool ValidateUser(string username, string password)
    {
        return FormsAuthentication.Authenticate(username, password);
    }

    #endregion
}

[XmlRpcMissingMapping(MappingAction.Ignore)]
public struct MediaObject
{
    public string name;
    public string type;
    public byte[] bits;
}

//[XmlRpcMissingMapping(MappingAction.Ignore)]
//public struct PostInfo
//{
//    public DateTime dateCreated;
//    public string description;
//    public string title;
//    public string[] categories;
//    public string permalink;
//    public object postid;
//    public string userid;
//    public string wp_slug;
//}