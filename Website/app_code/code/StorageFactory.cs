
    using System;
    using System.Collections.Generic;
    using System.Linq;

public class StorageFactory
    {
        static StorageFactory()
        {
            var storage = System.Configuration.ConfigurationManager.AppSettings.Get("blog:storage") ?? "Storage";
            var iType = typeof(IStorage);
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .FirstOrDefault(p => p.GetInterfaces().Contains(iType) && p.Name == storage);
            Instance = type == null ? new Storage() : (IStorage)Activator.CreateInstance(type);
        }

        public static IStorage Instance { get; set; } 
    }

    public interface IStorage
    {
        List<Post> GetAllPosts();
        void Save(Post post);
        void Delete(Post post);

    }
