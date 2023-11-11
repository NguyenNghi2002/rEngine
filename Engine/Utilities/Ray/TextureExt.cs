using Raylib_cs;
using System.Numerics;

namespace Engine
{
    public static class TextureExt
    {
		/// <summary>
		/// Get resolution of the texture.
		/// </summary>
		/// <param name="texture">Texture</param>
		/// <returns>a vector2D of texture's resolution</returns>
        public static Vector2 Scale(this Texture2D texture)
            => new Vector2(texture.width,texture.height);

		/// <summary>
		/// Get bound/rectange of texture
		/// </summary>
		/// <param name="texture">texture</param>
		/// <returns>source rectangle</returns>
        public static Rectangle Source(this Texture2D texture)
            => new Rectangle(0,0,texture.width,texture.height);

        public static Texture2D LoadScaledTextureFromImage(Image image,int scale)
        {
            var cloneImage = Raylib.ImageCopy(image);
            Raylib.ImageResizeNN(ref cloneImage, image.width * scale, image.height * scale);

            var texture = Raylib.LoadTextureFromImage(cloneImage);
            Raylib.UnloadImage(cloneImage);
            return texture;
        }
        public static Texture2D LoadScaledTexture(string fileName,int scale)
        {
            var image = Raylib.LoadImage(fileName);
            Raylib.ImageResizeNN(ref image,image.width * scale,image.height * scale);
            var texture = Raylib.LoadTextureFromImage(image);
            Raylib.UnloadImage(image);
            return texture;
        }
		public static Texture2D LoadTextureCropped(string fileName, Rectangle crop)
		{
			var image = Raylib.LoadImage(fileName);
			Raylib.ImageCrop(ref image, crop);
			var texture = Raylib.LoadTextureFromImage(image);
			Raylib.UnloadImage(image);
			return texture;
		}
		public static Texture2D Crop(Texture2D texture2D,Rectangle crop)
		{
			var image = Raylib.LoadImageFromTexture(texture2D);
			Raylib.ImageCrop(ref image,crop);
			var result = Raylib.LoadTextureFromImage(image);
			Raylib.UnloadImage(image);
			return result;
		}
    }
}
