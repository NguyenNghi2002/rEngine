#if false
using Engine.UI;
using Raylib_cs;
using System.Numerics;

namespace Engine
{

    public class rFont : IDisposable
    {
        //public static readonly rFont Default = new rFont(Raylib.GetFontDefault());


        private Font? _font;

        public float Size;
        public float Spacing;

        /// <summary>
        /// Characters Info
        /// </summary>
        public GlyphInfo[] GlyphInfos;

        public rFont(Font font)
        {
            var a = Raylib.GetFontDefault();
            Size = 10;//Core.Scene.GetRenderTextureScale().Y *(1/16f);
            Spacing = 2f;
            SetFont(font);
        }

        public void SetFont(Font font)
        {
            GlyphInfos = new GlyphInfo[font.glyphCount];

            for (int i = 0; i < font.glyphCount; i++)
            {
                GlyphInfo glyph = Raylib.GetGlyphInfo(font, i);
                GlyphInfos[i] = glyph;
            }
            _font = font;
        }

        public bool ContainChar(char character)
        {
            Raylib.GetGlyphInfo(_font.Value, character);
            return true;
        }

        public void DrawText(string text, Vector2 position, Vector2 origin = default, float angle = 0, Color? color = null)
        {
            Raylib.DrawTextPro(_font.Value, text, position, origin, angle, Size, Spacing, color ?? Color.WHITE);
        }

        public Vector2 MeasureText(string text)
            => Raylib.MeasureTextEx(_font.Value, text, Size, Spacing);


        public void Dispose()
        {
            if (_font.HasValue)
            {
                Raylib.UnloadFont(_font.Value);
                _font = null;
            }
        }

        public static implicit operator Font(rFont rFont) => rFont._font.Value;
    }


} 
#endif