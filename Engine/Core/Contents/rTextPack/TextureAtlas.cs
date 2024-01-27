using Engine;
using Raylib_cs;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.Enumeration;
using System.Net;
using System.Xml.Linq;

namespace Engine.Texturepacker
{

    public class TextureAtlas : AtlasDocument,IDisposable
    {

        public Texture2D? Texture;

        public string ImagePath;
        public int Width, Height;
        public int SpriteCount;
        public bool IsFont;
        public int FontSize;
        public Dictionary<string,Sprite> Sprites ;


        public TextureAtlas() { }

        public TextureAtlas(string path,int collumnCount,int rowCount)
        {
            Texture = ContentManager.Load<Texture2D>(Path.GetFileNameWithoutExtension(path), path);
            Debug.Assert(Texture.Value.id != 0);
            ImagePath = path;
            Width = Texture.Value.width;
            Height = Texture.Value.height;
            IsFont = false;
            FontSize = 0;

            var spriteWidth =  Texture.Value.width / collumnCount ;
            var spriteHeight =  Texture.Value.height / rowCount;

            Sprites = new Dictionary<string, Sprite>();
            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < collumnCount; x++)
                {
                    int px = x * spriteWidth;
                    int py = y * spriteHeight;
                    var sprite = new Sprite(this,$"{x},{y}",px,py,spriteWidth,spriteHeight);
                    Sprites.Add(sprite.Name, sprite);
                }
            }
                
        }
        public TextureAtlas(string path)
        {
            LoadFromData(ReadXml(path));
        }

        public void Dispose()
        {
            if(Texture != null)
            {
                if(ContentManager.TryGetName(Texture, out string textureName))
                    ContentManager.Unload<Texture2D>(textureName);
                Texture = null;
            }
        }

        private void LoadFromData(XDocument xDoc)
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
            Texture = ContentManager.Load<Texture2D>(Path.GetFileNameWithoutExtension(imageFullPath), imageFullPath);

        }

  
 
    }
}