using Raylib_cs;


namespace Engine.UI
{
	public struct DebugRectangleF
	{
		public Rectangle Rect;
		public Color Color;


		public DebugRectangleF(float x, float y, float width, float height, Color color)
		{
			Rect = new Rectangle(x, y, width, height);
			Color = color;
		}


		public void Set(float x, float y, float width, float height, Color color)
		{
			Rect.x = x;
			Rect.y = y;
			Rect.width = width;
			Rect.height = height;
			Color = color;
		}
	}
}