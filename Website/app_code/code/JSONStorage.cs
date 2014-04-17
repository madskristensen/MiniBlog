using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Newtonsoft.Json;

public class JSONStorage : IStorage
{
    private readonly string _folder = HostingEnvironment.MapPath("~/posts/");
    public List<Post> GetAllPosts()
    {
        if (HttpRuntime.Cache["posts"] == null)
            LoadPosts();

        if (HttpRuntime.Cache["posts"] != null)
        {
            return (List<Post>)HttpRuntime.Cache["posts"];
        }
        return new List<Post>();
    }

    public void Save(Post post)
    {
        string file = Path.Combine(_folder, post.ID + ".json");
        post.LastModified = DateTime.UtcNow;

        if (!File.Exists(file)) // New post
        {
            var posts = GetAllPosts();
            posts.Insert(0, post);
            posts.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
            HttpRuntime.Cache.Insert("posts", posts);
        }

        JsonTextWriter writer = new JsonTextWriter(new StreamWriter(file));
        JsonSerializer se = new JsonSerializer();
        se.Serialize(writer, post);
        writer.Flush();
        writer.Close();
    }

    public void Delete(Post post)
    {
        var posts = GetAllPosts();
        string file = Path.Combine(_folder, post.ID + ".json");
        File.Delete(file);
        posts.Remove(post);
    }

    private void LoadPosts()
    {
        if (!Directory.Exists(_folder))
            Directory.CreateDirectory(_folder);

        List<Post> list = new List<Post>();

        foreach (string file in Directory.GetFiles(_folder, "*.json", SearchOption.TopDirectoryOnly))
        {
            JsonTextReader reader = new JsonTextReader(new StreamReader(file));
            Post post = new JsonSerializer().Deserialize<Post>(reader);
            reader.Close();
            list.Add(post);
        }

        if (list.Count > 0)
        {
            list.Sort((p1, p2) => p2.PubDate.CompareTo(p1.PubDate));
            HttpRuntime.Cache.Insert("posts", list);
        }
    }

}