#define Singleton
using Engine.Texturepacker;
using Engine.UI;
using Raylib_cs;
using System.Reflection.Metadata.Ecma335;

namespace Engine
{



    public class ContentManager : GlobalManager
    {
#if Singleton
        public static ContentManager Instance;
#endif
        /** Collection  **/


        //public Dictionary<string , Resource> Resources = new Dictionary<string,Resource>();
        public Dictionary<string,Dictionary< Type, object>> Resources = new ();


        /// <summary>
        /// Loader for specific Resource<br/><br/>
        /// <see cref="Type"/> : type of resource <br/>
        /// <see cref="IResourceHandler"/> : Loader for specific Resource<br/>
        /// </summary>
        public Dictionary<Type, IResourceHandler> ResourceHandlers = new Dictionary<Type, IResourceHandler>();

        public ContentManager()
        {
            Instance ??= this;

            AddDefaultLoaders();
        }

        void AddDefaultLoaders()
        {
            ResourceHandlers.Add(typeof(Texture2D)    , new Texture2DLoader());
            ResourceHandlers.Add(typeof(Sound)      , new SoundLoader());
            ResourceHandlers.Add(typeof(Skin)        , new SkinLoader()); // Incompleted
            ResourceHandlers.Add(typeof(TextureAtlas), new TextureAtlasLoader());
        }

        /// <summary>
        /// Add resource loader for content manager to load and save
        /// </summary>
        /// <typeparam name="Tresource">Type that inherit from <see cref="Resource"/></typeparam>
        /// <param name="loader">Loader that return same type as <typeparamref name="Tresource"/></param>
        public static void AddHandler<Tresource>(IResourceHandler loader) 
        {
            Instance.ResourceHandlers.Add(typeof(Tresource), loader);
        }
        /// <summary>
        /// Remove resource loader
        /// </summary>
        /// <typeparam name="Tresource">Type that inherit from <see cref="Resource"/></typeparam>
        /// <returns>True if loader successfully removed</returns>
        public static bool RemoveHandler<Tresource>() 
        {
            return Instance.ResourceHandlers.Remove(typeof(Tresource));
        }

#if Singleton

        public static void UnloadAll()
        {
            var i = Instance;
            var handlableResources = Instance.Resources.Where(c => i.ResourceHandlers.ContainsKey(c.Value.GetType()));
            var dictionaryHandlers = handlableResources.Select(c => new KeyValuePair<IResourceHandler, object>(i.ResourceHandlers[c.GetType()], c));
            foreach (KeyValuePair<IResourceHandler, object> o in dictionaryHandlers)
            {
                o.Key.Unload(o.Value);
            }
            Instance.Resources.Clear();    
            //TODO: This should clear all contents cause memory leak
        }
        public static bool Unload<T>(string name)
        {
            if (TryGet<T>(name,out T? content))
            {
                var type = typeof(T);
                if (Instance.ResourceHandlers.TryGetValue(type,out IResourceHandler? handler))
                    handler.Unload(type);
                return true;
            }
            return false;
        }

        public static T Load<T>(string resourceName, Func<T> customloadhandler)
        {
            //final content
            object content = null;
            var nameDict = Instance.Resources;

            //Check if the name already exist
            if (Instance.Resources.TryGetValue(resourceName, out Dictionary<Type,object>? contentDic))
            {
                //Check if the type already exist
                if (contentDic.TryGetValue(typeof(T), out content))
                {
                    Debugging.Log($"Resource {resourceName} is already loaded", Debugging.LogLevel.Warning);
                }
                else // type is not contained in contentDic
                {

                    content = customloadhandler.Invoke();
                    contentDic.Add(typeof(T),content);
                }
            }
            else
            {
                /// custume invoke could have sub invoke in it so becareful
                content = customloadhandler.Invoke();

                var contentDictionary = new Dictionary<Type, object>();
                if (Instance.Resources.TryAdd(resourceName, contentDictionary))
                {
                    /// If new name was added, then add content to fresh new dictionary
                    contentDictionary.Add(typeof(T),content);
                }
                else
                {
                    /// If name was added, that's mean name has dictionary in it already,
                    /// just add new type / content
                    Instance.Resources[resourceName].Add(typeof(T),content);
                    //Instance.Resources[resourceName].Clear();
                }
            }
            return (T)content;
        } 
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resourceName">name your resource</param>
        /// <param name="path">file path</param>
        /// <returns>loaded <typeparamref name="T"/></returns>
        public static T Load<T>(string resourceName, string path, bool ignoreWarning = false) 
        {
            return Load<T>(resourceName, () =>
            {
                return (T)Instance.ResourceHandlers[typeof(T)].Load(path);
            });
#if false
            object content = null;

            ///[GUARD]
            ///Check if content is availabled
            if (Instance.Resources.TryGetValue(resourceName, out content))
            {
                if (!ignoreWarning)
                    Debugging.Log($"Resource {resourceName} is already loaded", Debugging.LogLevel.Warning);
                return (T)content;
            }

            /// Load 
            //TODO: add throw error for object that don't have loading handler
            content = Instance.ResourceHandlers[typeof(T)].Load(path);

            Instance.Resources.Add(resourceName, content);

            return (T)content; 
#endif
        }


        public static T Get<T>(string name) 
            => (T)Instance.Resources[name][typeof(T)] ;

        public static bool TryGet<T>(string name,out T? found) 
        {
            found = default;
            if(Instance.Resources.TryGetValue(name, out var foundDic) 
                && foundDic.TryGetValue(typeof(T), out object? content))
            {
                found = (T?)content ;
                return true;
            }

            return false;
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