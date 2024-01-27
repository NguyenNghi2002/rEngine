using Engine.Texturepacker;
using Engine.UI;
using Newtonsoft.Json;
using Raylib_cs;
using System.Diagnostics;

namespace Engine
{
    public class FontLoader : IResourceHandler
    {
        public object Load(string path)
        {
            return Raylib.LoadFont(path);
        }
        public void Unload(object resource)
        {
            if (resource is Font texture2D)
                Raylib.UnloadFont(texture2D);
        }
    }
    public class Texture2DLoader : IResourceHandler
    {
        public object Load(string path)
        {
            Debug.Assert(Path.GetExtension(path) != ".jpg","Not suppported");
            return Raylib.LoadTexture(path);
        }
        public void Unload(object resource)
        {
            if(resource is Texture2D texture2D)
            Raylib.UnloadTexture(texture2D);
        }
    }
    public class SoundLoader : IResourceHandler
    {
        object IResourceHandler.Load(string path) => Raylib.LoadSound(path);
        void IResourceHandler.Unload(object resource)
        {
            if (resource is Sound sound) 
                Raylib.UnloadSound(sound);
        }
    }

    public class SkinLoader : IResourceHandler
    {
        public object Load(string path)
        {
            var a = File.ReadAllText(path);
            var xDoc = JsonConvert.DeserializeObject<dynamic>(a);

            return new Skin();
        }

        void IResourceHandler.Unload(object resource)
        {
            Skin? skin = resource as Skin;
            // No need to unload
        }
    }

    public class TextureAtlasLoader : IResourceHandler
    {
        public object Load(string path)
            =>new TextureAtlas(path);

        public void Unload(object resource)
        {

            (resource as TextureAtlas)?.Dispose();
        }
    }

}