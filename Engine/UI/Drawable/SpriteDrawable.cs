using Raylib_cs;
using System.Numerics;
using Engine.UI;
using Engine.Texturepacker;

namespace Engine.UI
{
    public class SpriteDrawable : IDrawable
    {
        public Sprite Sprite;
        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }

        public SpriteDrawable(float minWidth,float minHeight,Sprite sprite)
        {
            Insist.IsNotNull(sprite, $"Image is null is null ");
            this.MinWidth = minWidth;
            this.MinHeight = minHeight;
            this.Sprite = sprite;
        }
        public SpriteDrawable(Texture2D texture) : this(texture.width,texture.height,new Sprite(texture)) { }
        public SpriteDrawable(Sprite sprite) : this(sprite.SourceWidth,sprite.SourceHeight,sprite) { }
        public void Draw(float x, float y, float width, float height, Color color)
        {
            if(Sprite != null)
            Raylib.DrawTexturePro(Sprite.Atlas.Texture.Value, new Rectangle(Sprite.PositionX, Sprite.PositionY, Sprite.SourceWidth, Sprite.SourceHeight), new Rectangle(x, y, width, height), Vector2.Zero, 0, color);
        }

        public void SetPadding(float top, float bottom, float left, float right)
        {
            LeftWidth = left;
            RightWidth = right;
            TopHeight = top;
            BottomHeight = bottom;
        }
    }
}