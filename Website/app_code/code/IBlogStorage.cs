using System.Collections.Generic;

interface IBlogStorage
{
    List<Post> GetAllPosts();
    void Delete(Post post);
    void Save(Post post);
}
