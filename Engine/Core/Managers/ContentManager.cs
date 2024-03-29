﻿#define Singleton
using Engine.Texturepacker;
using Engine.TiledSharp;
using Engine.UI;
using Raylib_cs;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace Engine
{



    public class ContentManager : GlobalManager
    {
#if Singleton
        public static ContentManager Instance;
#endif
        /** Collection  **/


        //public Dictionary<string , Resource> Resources = new Dictionary<string,Resource>();
        //public Dictionary<string,Dictionary< Type, object>> Resources = new ();
        public Dictionary<Type,Dictionary< string, object>> Resources = new ();


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
            ResourceHandlers.Add(typeof(Font)           , new FontLoader());
            ResourceHandlers.Add(typeof(Texture2D)      , new Texture2DLoader());
            ResourceHandlers.Add(typeof(Sound)          , new SoundLoader());
            ResourceHandlers.Add(typeof(Music)          , new MusicLoader());
            ResourceHandlers.Add(typeof(Skin)           , new SkinLoader()); // Incompleted
            ResourceHandlers.Add(typeof(TextureAtlas)   , new TextureAtlasLoader());
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

        /// <summary>
        /// Remove content from memory and Unload it if it have handler
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Unload<T>(string name)
        {
            if (TryGet<T>(name,out T? content))
            {
                var type = typeof(T);
                if (Instance.ResourceHandlers.TryGetValue(type, out IResourceHandler? handler))
                    handler.Unload(content);
                else
                    Debugging.Log($"{name} typeof {type} doesn't have handler");
                
                Instance.Resources[type].Remove(name); /** This is for <Type,<string,Object>> **/

                ///If dictionary have no type, then remove
                if (Instance.Resources[type].Count == 0)
                    Instance.Resources.Remove(type);

                return true;
            }
            return false;
        }
#if false
        public static bool Unload<T>(T content)
        {
            foreach (var resourceKeyPair in Instance.Resources)
            {
                var name = resourceKeyPair.Key;
                var resourceDictionary = resourceKeyPair.Value;
                if (resourceDictionary.ContainsValue(content))
                {
                    resourceDictionary[typeof(T)]
                    return true;
                }
            }
            return false;
        } 
#endif

        /// <summary>
        /// Get content name base of instance and type
        /// </summary>
        /// <typeparam name="T">content type</typeparam>
        /// <param name="content">content instance</param>
        /// <param name="foundName"></param>
        /// <returns></returns>
        public static bool TryGetName<T>(T content,out string foundName)
        {
            foundName = string.Empty;
            if (Instance.Resources.TryGetValue(typeof(T),out var typeDictionary))
            {
                foundName = typeDictionary
                    .Where(p=>p.Value == (object)content)
                    .Select(p=>p.Key)
                    .First()
                    ;
                return true;
            }

            return false;
        }



        public static T Load<T>(string name, Func<T> customloadhandler)
        {
            //final content
            object content = null;
            Type type = typeof(T);
            //Check if the name already exist
            if (Instance.Resources.TryGetValue(type, out var sub))
            {
                Debugging.Log($"Found {name}", Debugging.LogLevel.System);

                //Check if the type already exist
                if (sub.TryGetValue(name, out content))
                {
                    Debugging.Log($"Resource {name} is already loaded", Debugging.LogLevel.Warning);
                }
                else // type is not contained in contentDic
                {

                    content = customloadhandler.Invoke();
                    sub.Add(name,content);
                    Debugging.Log($"Assign : {typeof(T)} -> {name}", Debugging.LogLevel.System);
                }
            }
            else
            {
                Debugging.Log($"Can't Find {name}", Debugging.LogLevel.System);

                /// custume invoke could have sub invoke in it so becareful
                content = customloadhandler.Invoke();

                var newSub = new Dictionary<string, object>();
                if (Instance.Resources.TryAdd(type, newSub))
                {
                    /// If new name was added, then add content to fresh new dictionary
                    newSub.Add(name,content);
                }
                else /// This happen when custom load have sub-custom load and it can cause error thrown 
                {
                    Debugging.Log($"Resource {name} type of {typeof(T)} is already loaded", Debugging.LogLevel.Comment);
                    /// If name was added, that's mean name has dictionary in it already,
                    /// just add new type / content
                    Instance.Resources[type].Add(name,content);
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
        public static T Load<T>(string resourceName, string path) 
        {
            Debug.Assert( Instance.ResourceHandlers.ContainsKey(typeof(T)),"no handler found for this type");

            return Load<T>(resourceName, () =>
            {
                return (T)Instance.ResourceHandlers[typeof(T)].Load(path);
            });
        }


        public static T Get<T>(string name) 
            => (T)Instance.Resources[typeof(T)][name];

        public static bool TryGet<T>(string name,out T? found) 
        {
            found = default;
            if(Instance.Resources.TryGetValue(typeof(T), out var sub) 
                && sub.TryGetValue(name, out object? content))
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