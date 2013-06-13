using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

/// <summary>
/// Summary description for Comment
/// </summary>
public class Comment
{
    public Comment()
    {
        ID = Guid.NewGuid().ToString();
        PubDate = DateTime.UtcNow;
    }

    public string ID { get; set; }
    public string Author { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
    public string Content { get; set; }
    public DateTime PubDate { get; set; }
    public string Ip { get; set; }
    public string UserAgent { get; set; }

    public string GravatarUrl (int imgSize)
    {
        var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(Email.ToLowerInvariant(), "MD5").ToLower();

        // build Gravatar Image URL
        var imageUrl = string.Format("http://www.gravatar.com/avatar/{0}?s={1}&d=mm&r=g", hash, imgSize);

        return imageUrl;
    }
}