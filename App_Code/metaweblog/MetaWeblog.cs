using CookComputing.XmlRpc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;

public interface IMetaWeblog
{
    #region MetaWeblog API

    [XmlRpcMethod("metaWeblog.newPost")]
    string AddPost(string blogid, string username, string password, PostInfo post, bool publish);

    [XmlRpcMethod("metaWeblog.editPost")]
    bool UpdatePost(string postid, string username, string password, PostInfo post, bool publish);

    [XmlRpcMethod("metaWeblog.getPost")]
    PostInfo GetPost(string postid, string username, string password);

    //[XmlRpcMethod("metaWeblog.getCategories")]
    //CategoryInfo[] GetCategories(string blogid, string username, string password);

    [XmlRpcMethod("metaWeblog.getRecentPosts")]
    PostInfo[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts);

    [XmlRpcMethod("metaWeblog.newMediaObject")]
    MediaObjectInfo NewMediaObject(string blogid, string username, string password,
        MediaObject mediaObject);

    #endregion

    #region Blogger API

    [XmlRpcMethod("blogger.deletePost")]
    [return: XmlRpcReturnValue(Description = "Returns true.")]
    bool DeletePost(string key, string postid, string username, string password, bool publish);

    [XmlRpcMethod("blogger.getUsersBlogs")]
    BlogInfo[] GetUsersBlogs(string key, string username, string password);

    [XmlRpcMethod("blogger.getUserInfo")]
    UserInfo GetUserInfo(string key, string username, string password);

    #endregion
}

public class MetaWeblog : XmlRpcService, IMetaWeblog
{
        #region IMetaWeblog Members

    string IMetaWeblog.AddPost(string blogid, string username, string password, PostInfo post, bool publish)
    {
        if (ValidateUser(username, password))
        {
            Post newPost = new Post()
            {
                Content = post.description,
                Title = post.title,
                PubDate = post.dateCreated > DateTime.MinValue ? post.dateCreated.ToUniversalTime() : DateTime.UtcNow,
                Slug = PostHandler.CreateSlug(post.title),
            };

            newPost.Save();

            return newPost.ID;
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    bool IMetaWeblog.UpdatePost(string postid, string username, string password, PostInfo post, bool publish)
    {
        if (ValidateUser(username, password))
        {
            Post newPost = Post.Posts.FirstOrDefault(p => p.ID == postid);

            if (newPost != null)
            {
                newPost.Content = post.description;
                newPost.Title = post.title;
                newPost.PubDate = post.dateCreated;

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
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    PostInfo IMetaWeblog.GetPost(string postid, string username, string password)
    {
        if (ValidateUser(username, password))
        {
            Post newPost = Post.Posts.FirstOrDefault(p => p.ID == postid);

            if (newPost != null)
            {
                PostInfo post = new PostInfo()
                {
                    title = newPost.Title,
                    description = newPost.Content,
                    wp_slug = newPost.Slug,
                    dateCreated = newPost.PubDate,
                    postid = newPost.ID,
                };

                return post;
            }
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

    PostInfo[] IMetaWeblog.GetRecentPosts(string blogid, string username, string password, int numberOfPosts)
    {
        if (ValidateUser(username, password))
        {
            List<PostInfo> list = new List<PostInfo>();

            var posts = Post.Posts.Take(numberOfPosts);
            foreach (var post in posts)
            {
                PostInfo info = new PostInfo()
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
        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    MediaObjectInfo IMetaWeblog.NewMediaObject(string blogid, string username, string password, MediaObject mediaObject)
    {
        if (ValidateUser(username, password))
        {            
            string folder = Context.Server.MapPath("~/posts/files/");
            string fileName = Guid.NewGuid() + Path.GetExtension(mediaObject.name);
            string path = Path.Combine(folder, fileName);

            File.WriteAllBytes(path, mediaObject.bits);

            return new MediaObjectInfo() {
                url = VirtualPathUtility.ToAbsolute("~/posts/files/" + fileName)
            };            
        }
        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    BlogInfo[] IMetaWeblog.GetUsersBlogs(string key, string username, string password)
    {
        if (ValidateUser(username, password))
        {
            BlogInfo blog = new BlogInfo()
            {
                blogid = "1",
                blogName = ConfigurationManager.AppSettings.Get("blog:name"),
                url = Context.Request.Url.Scheme + "://" + Context.Request.Url.Authority
            };

            return new[] { blog };
        }

        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    UserInfo IMetaWeblog.GetUserInfo(string key, string username, string password)
    {
        if (ValidateUser(username, password))
        {
            UserInfo info = new UserInfo();

            // TODO: Implement your own logic to get user info objects and set the info

            return info;
        }
        throw new XmlRpcFaultException(0, "User is not valid!");
    }

    #endregion

    #region Private Methods

    private bool ValidateUser(string username, string password)
    {
        return FormsAuthentication.Authenticate(username, password);
    }

    #endregion
}