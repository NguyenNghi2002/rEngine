#define Singleton
using Engine.Texturepacker;
using Engine.UI;
using Raylib_cs;

namespace Engine
{



    public class ContentManager : GlobalManager
    {
#if Singleton
        public static ContentManager Instance;
#endif
        /** Collection  **/


        public Dictionary<string , Resource> Resources = new Dictionary<string,Resource>();

        /// <summary>
        /// Loader for specific Resource<br/><br/>
        /// <see cref="Type"/> : type of resource <br/>
        /// <see cref="IResourceLoader"/> : Loader for specific Resource<br/>
        /// </summary>
        public Dictionary<Type, IResourceLoader> Loaders = new Dictionary<Type, IResourceLoader>();

        public ContentManager()
        {
            Instance ??= this;

            AddDefaultLoaders();
        }

        void AddDefaultLoaders()
        {
            Loaders.Add(typeof(rTexture)    , new TextureLoader());
            Loaders.Add(typeof(rSound)    , new SoundLoader());
            Loaders.Add(typeof(Skin)        , new SkinLoader()); // Incompleted
            Loaders.Add(typeof(TextureAtlas), new TextureAtlasLoader());
        }

        /// <summary>
        /// Add resource loader for content manager to load and save
        /// </summary>
        /// <typeparam name="Tresource">Type that inherit from <see cref="Resource"/></typeparam>
        /// <param name="loader">Loader that return same type as <typeparamref name="Tresource"/></param>
        public static void AddResourceLoader<Tresource>(IResourceLoader loader) where Tresource : Resource
        {
            Instance.Loaders.Add(typeof(Tresource), loader);
        }
        /// <summary>
        /// Remove resource loader
        /// </summary>
        /// <typeparam name="Tresource">Type that inherit from <see cref="Resource"/></typeparam>
        /// <returns>True if loader successfully removed</returns>
        public static bool RemoveResourceLoader<Tresource>() where Tresource : Resource
        {
            return Instance.Loaders.Remove(typeof(Tresource));
        }

#if Singleton

        public static void UnloadAll()
        {
            foreach (var asset in Instance.Resources)
                asset.Value.Dispose();
            Instance.Resources.Clear();    
        }
        public static bool Unload(string name)
        {
            if (Instance.Resources.Remove(name, out Resource content))
            {
                content.Dispose();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName">name your resource</param>
        /// <param name="path">file path</param>
        /// <returns>loaded <typeparamref name="T"/></returns>
        public static T Load<T>(string resourceName, string path) where T : Resource
        {
            Resource content = null;

            ///[GUARD]
            ///Check if content is availabled
            if (Instance.Resources.TryGetValue(resourceName,out content))
            {
                Debugging.Log($"Resource {resourceName} is already loaded", Debugging.LogLevel.Warning);
                return (T)content;
            } 


            /// Load 
            content = Instance.Loaders[typeof(T)].Load(path);

            Instance.Resources.Add(resourceName, content);

            return (T)content;
        }

        public static T? GetOrNull<T>(string name) where T : Resource
        {
            if(TryGet(name,out T? res))
                return res;
            return null;
        }
        public static T Get<T>(string name) where T : Resource
            => (T)Instance.Resources[key: name];

        public static bool TryGet<T>(string name,out T? found) where T : Resource
        {
            var result = Instance.Resources.TryGetValue(name, out Resource foundResource);
            found = foundResource as T;
            return result;
        }

#else
        public Resource this[string name] => resources[name];

        public T Load<T>(string name,string path) where T : Resource
        {
            Resource content = Loaders[typeof(T)].LoadContent(path);
            content.FilePath = path;

            resources.Add(name, content);

            return (T)content;
        }
        public T Get<T>(string name) where T : Resource
            =>(T)resources[name];


        public ContentManager Unload<T>(string name)
        {
            if(resources.Remove(name,out Resource content))
                content.Dispose();
            return this;
        }
        public ContentManager UnloadAll<T>()
        {
            foreach (var content in resources)
                content.Value.Dispose();
            return this;
        }

        

#endif

    }


}