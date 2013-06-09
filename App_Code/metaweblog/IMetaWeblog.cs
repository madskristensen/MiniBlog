//using CookComputing.XmlRpc;

//public interface IMetaWeblog
//{
//    #region MetaWeblog API

//    [XmlRpcMethod("metaWeblog.newPost")]
//    string AddPost(string blogid, string username, string password, PostInfo post, bool publish);

//    [XmlRpcMethod("metaWeblog.editPost")]
//    bool UpdatePost(string postid, string username, string password, PostInfo post, bool publish);

//    [XmlRpcMethod("metaWeblog.getPost")]
//    PostInfo GetPost(string postid, string username, string password);

//    //[XmlRpcMethod("metaWeblog.getCategories")]
//    //CategoryInfo[] GetCategories(string blogid, string username, string password);

//    [XmlRpcMethod("metaWeblog.getRecentPosts")]
//    PostInfo[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts);

//    [XmlRpcMethod("metaWeblog.newMediaObject")]
//    MediaObjectInfo NewMediaObject(string blogid, string username, string password,
//        MediaObject mediaObject);

//    #endregion

//    #region Blogger API

//    [XmlRpcMethod("blogger.deletePost")]
//    [return: XmlRpcReturnValue(Description = "Returns true.")]
//    bool DeletePost(string key, string postid, string username, string password, bool publish);

//    [XmlRpcMethod("blogger.getUsersBlogs")]
//    BlogInfo[] GetUsersBlogs(string key, string username, string password);

//    [XmlRpcMethod("blogger.getUserInfo")]
//    UserInfo GetUserInfo(string key, string username, string password);

//    #endregion
//}