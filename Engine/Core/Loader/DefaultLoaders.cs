using Engine.Texturepacker;
using Engine.UI;
using Newtonsoft.Json;
namespace Engine
{
    public class TextureLoader : IResourceLoader
    {
        public Resource Load(string path)
            =>rTexture.Load(path);
    }
    public class SoundLoader : IResourceLoader
    {
        Resource IResourceLoader.Load(string path) => rSound.Load(path);
    }

    public class SkinLoader : IResourceLoader
    {
        public Resource Load(string path)
        {
            var a = File.ReadAllText(path);
            var xDoc = JsonConvert.DeserializeObject<dynamic>(a);

            return new Skin();
        }
    }

    public class TextureAtlasLoader : IResourceLoader
    {
        public Resource Load(string path)
            =>new TextureAtlas(path);
    }

}