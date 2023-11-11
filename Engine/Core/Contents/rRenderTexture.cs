using Raylib_cs;

namespace Engine
{
    public class rRenderTexture : Resource
    {
        private RenderTexture2D _renderTexture;
        private rTexture _texture;

        public TextureFilter Filter
        {
            set => SetFilter(value);
        }
        public TextureWrap Warp
        {
            set => SetWarp(value);
        }
        public rTexture Texture => _texture;
        public rRenderTexture SetFilter(TextureFilter filter)
        {
            Raylib.SetTextureFilter(_texture,filter);
            return this;
        }
        public rRenderTexture SetWarp(TextureWrap warpmode)
        {
            Raylib.SetTextureWrap(_renderTexture.texture,warpmode);
            return this;
        }
        public void Dispose()
        {
            _texture.Dispose();
            _texture = null;
            Raylib.UnloadRenderTexture(_renderTexture);
        }

        public static implicit operator RenderTexture2D(rRenderTexture rRenderTexture)
        {
            return rRenderTexture._renderTexture;
        }
        public static rRenderTexture Load(int width, int height)
        {
            var renderTex = Raylib.LoadRenderTexture(width, height);
            return new rRenderTexture()
            {
                _renderTexture = renderTex,
                _texture = new rTexture(renderTex.texture),
                Filter = TextureFilter.TEXTURE_FILTER_POINT,
                Warp = TextureWrap.TEXTURE_WRAP_CLAMP,
            };
        }

    }
}