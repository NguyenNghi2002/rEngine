using Engine;
using Raylib_cs;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml.Linq;

namespace Engine.Texturepacker
{

    public class TextureAtlas : AtlasDocument,IDisposable
    {

        public rTexture Texture;

        public string ImagePath;
        public int Width, Height;
        public int SpriteCount;
        public bool IsFont;
        public int FontSize;
        public Dictionary<string,Sprite> Sprites;


        public TextureAtlas() { }
        public TextureAtlas(string path)
        {
            Load(ReadXml(path));
        }

        public void Dispose()
        {
            Texture.Dispose();  
        }

        private void Load(XDocument xDoc)
        {
            var atlas= xDoc.Element("AtlasTexture");

            ImagePath = (string?)atlas.Attribute("imagePath") ;
            Width = (int?)atlas.Attribute("width") ?? 0;
            Height = (int?)atlas.Attribute("height") ?? 0;
            SpriteCount = (int?)atlas.Attribute("spriteCount") ?? 0;
            IsFont = (bool?)atlas.Attribute("isFont") ?? false;
            FontSize = (int?)atlas.Attribute("fontSize") ?? 0;

            Sprites = new Dictionary<string, Sprite>();
            foreach (var xelement in atlas.Elements("Sprite"))
            {
                var sprite = new Sprite(this,xelement);
                Sprites.Add(sprite.Name,sprite);
            }

            var imageFullPath = Path.Combine(Directory, ImagePath);
            Texture = rTexture.Load(imageFullPath);

        }

    }
}