using Raylib_cs;

namespace Engine.UI
{
	public class TextTooltip : Tooltip
	{
#if false
        public TextTooltip(string text, Element targetElement, Skin skin, string styleName = null) : this(text,
            targetElement, skin.Get<TextTooltipStyle>(styleName))
        {
        } 
#endif


        public TextTooltip(string text, Element targetElement, TextTooltipStyle style) : base(null, targetElement)
		{
			var label = new Label(text, style.LabelStyle);
			_container.SetElement(label);
			SetStyle(style);
		}


		public TextTooltip SetStyle(TextTooltipStyle style)
		{
			_container.GetElement<Label>().SetStyle(style.LabelStyle);
			_container.SetBackground(style.Background);
			return this;
		}
	}


	public class TextTooltipStyle
	{
		public LabelStyle LabelStyle;

		/** Optional. */
		public IDrawable Background;


		public TextTooltipStyle()
		{
		}

		public static TextTooltipStyle CreateRaylib()
        {
			return new TextTooltipStyle()
			{
				LabelStyle = new LabelStyle(Raylib.GetFontDefault(), Color.RED),
				Background = new PrimitiveDrawable(Color.BLACK)
			};
        }


		public TextTooltipStyle(LabelStyle label, IDrawable background)
		{
			LabelStyle = label;
			Background = background;
		}
	}
}