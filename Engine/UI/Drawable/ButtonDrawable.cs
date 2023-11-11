using Raylib_cs;

namespace Engine.UI
{
    public class ButtonDrawable : IDrawable
    {
        #region IDrawable Properties
        public float LeftWidth { get; set; }
        public float RightWidth { get; set; }
        public float TopHeight { get; set; }
        public float BottomHeight { get; set; }
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }
		#endregion

		public Color? Color,OutlineColor;
        public bool MultiplyColor  = false;
        public float LineWidth  = 5f;

        public ButtonDrawable(Color? color,Color? outlineColor)
        {
			Color = color;
			OutlineColor = outlineColor;
        }
		public void Draw(float x, float y, float width, float height, Color color)
		{
			var col = Color.HasValue ? Color.Value : color;
            var outlineCol = OutlineColor.HasValue ? OutlineColor.Value : color;
            if (MultiplyColor)
            {
                if (color.a != 255)
                {
                    float c = (color.a / 255f);
                    col.r = (byte)(col.r * c);
                    col.g = (byte)(col.g * c);
                    col.b = (byte)(col.b * c);
                    col.a = (byte)(col.a * c);
                    //col *= (color.a / 255f);
                }
                if (col.a != 255)
                {
                    float c = (color.a / 255f);
                    col.r = (byte)(col.r * c);
                    col.g = (byte)(col.g * c);
                    col.b = (byte)(col.b * c);
                    col.a = (byte)(col.a * c);

                    //col *= (col.a / 255f);
                }
                if (outlineCol.a != 255)
                {
                    float c = (col.a / 255f);
                    outlineCol.r = (byte)(outlineCol.r * c);
                    outlineCol.g = (byte)(outlineCol.g * c);
                    outlineCol.b = (byte)(outlineCol.b * c);
                    outlineCol.a = (byte)(outlineCol.a * c);

                } 
            }

			var rec = new Rectangle(x, y, width, height);
			Raylib.DrawRectangleRec(rec, col);
			Raylib.DrawRectangleLinesEx(rec, LineWidth, outlineCol);
		}

		public void SetPadding(float top, float bottom, float left, float right)
        {
            throw new NotImplementedException();
        }


	}

}