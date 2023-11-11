using Raylib_cs;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Numerics;

namespace Engine
{
    [DebuggerDisplay("Id = {ID}, Width = {Width}, Height = {Height}")]
    public class rTexture : Resource
    {
        Texture2D? _texture2D;

        public Texture2D RawData => _texture2D.Value;
        public int Width => _texture2D.Value.width;
        public int Height => _texture2D.Value.height;
        public uint ID => _texture2D.Value.id;
        internal rTexture(Texture2D texture)
        {
            _texture2D = texture;
        }
        public override void Dispose()
        {
            if (_texture2D.HasValue)
            {
                Raylib.UnloadTexture(_texture2D.Value);
                _texture2D = null;
            }
        }

        public Vector2 Scale() => new Vector2(Width,Height);

        public static implicit operator Texture2D(rTexture rTexture) => rTexture._texture2D.Value;
        public static implicit operator uint(rTexture rTexture) => rTexture._texture2D.Value.id;

        public static rTexture Load(string filePath)
        {
            Insist.IsTrue(Path.HasExtension(filePath),$"{filePath} doesn't have extension");

            return new rTexture(Raylib.LoadTexture(filePath));
        }
    }
}