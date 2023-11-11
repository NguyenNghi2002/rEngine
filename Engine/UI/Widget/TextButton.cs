using Raylib_cs;
using System;
using System.Diagnostics;

namespace Engine.UI
{
	public class TextButton : Button
	{
		Label label;
		TextButtonStyle style;


		public TextButton(string text, TextButtonStyle style) : base(style)
		{
			SetStyle(style);
			label = new Label(text, style.Font, style.FontColor, style.Font.Size);
			label.SetAlignment(UI.Align.Center);

			Add(label).Expand().Fill();
			SetSize(PreferredWidth, PreferredHeight);
		}


        public TextButton(string text, Skin skin, string styleName = null) : this(text,
        skin.Get<TextButtonStyle>(styleName))
        {
        } 

        public override void SetStyle(ButtonStyle style)
		{
			Insist.IsTrue(style is TextButtonStyle, "style must be a TextButtonStyle");

			base.SetStyle(style);
			this.style = (TextButtonStyle) style;

			if (label != null)
			{
				var textButtonStyle = (TextButtonStyle) style;
				var labelStyle = label.GetStyle();
				labelStyle.Font = textButtonStyle.Font;
				labelStyle.FontColor = textButtonStyle.FontColor;
				labelStyle.FontScale = textButtonStyle.Font.Size;
				label.SetStyle(labelStyle);
			}
		}
		public new TextButtonStyle GetStyle()
		{
			return style;
		}

		public TextButton SetFontScale(float scale)
        {
			var label = GetLabel();
			label.SetFontScale(scale);
			return this;
		}



		public override void Draw( float parentAlpha)
		{
			Color? fontColor = null;
			if (_isDisabled && style.DisabledFontColor.HasValue)
				fontColor = style.DisabledFontColor;
			else if (_mouseDown && style.DownFontColor.HasValue)
				fontColor = style.DownFontColor;
			else if (IsChecked &&
			         (!_mouseOver && style.CheckedFontColor.HasValue ||
			          _mouseOver && style.CheckedOverFontColor.HasValue))
				fontColor = (_mouseOver && style.CheckedOverFontColor.HasValue)
					? style.CheckedOverFontColor
					: style.CheckedFontColor;
			else if (_mouseOver && style.OverFontColor.HasValue)
				fontColor = style.OverFontColor;
			else
				fontColor = style.FontColor;

			if (fontColor != null)
				label.GetStyle().FontColor = fontColor.Value;
			
			base.Draw( parentAlpha);
			//Raylib.DrawTextPro(style.Font,GetText(),,);
		}


		public Label GetLabel()
		{
			return label;
		}


		public Cell GetLabelCell()
		{
			return GetCell(label);
		}


		public TextButton SetText(String text)
		{
			label.SetText(text);
			return this;
		}


		public string GetText()
		{
			return label.GetText();
		}


		public override string ToString()
		{
			return string.Format("[TextButton] text: {0}", GetText());
		}
	}


	/// <summary>
	/// The style for a text button
	/// </summary>
	public class TextButtonStyle : ButtonStyle
	{
		public rFont Font;

		/** Optional. */
		public Color FontColor = Color.WHITE;
		public Color? DownFontColor, OverFontColor, CheckedFontColor, CheckedOverFontColor, DisabledFontColor;


		public TextButtonStyle()
		{
			Font = rFont.Default;
		}


		public TextButtonStyle(IDrawable up, IDrawable down, IDrawable over, rFont font) : base(up, down, over)
		{
			Font = font ?? rFont.Default;
		}


		public TextButtonStyle(IDrawable up, IDrawable down, IDrawable over) : this(up, down, over,
			rFont.Default)
		{
		}

        #region Factory
        public static TextButtonStyle CreateRaylib()
        {
			var btt = Create(
				Raylib.GetColor(0xc9c9c9ff), Raylib.GetColor(0x838383ff),
				Raylib.GetColor(0x97e8ffff), Raylib.GetColor(0x0492c7ff),
				Raylib.GetColor(0xc9effeff), Raylib.GetColor(0x5bb2d9ff),
				Raylib.GetColor(0x838383ff));
			btt.DownFontColor = Raylib.GetColor(0x0492c7ff);
			btt.OverFontColor = Raylib.GetColor(0x5bb2d9ff);
			return btt;
		}
        public static TextButtonStyle Create(
        Color upColor, Color outlineUpColor,
        Color downColor, Color outlineDownColor,
        Color overColor, Color outlineOverColor, Color? fontColor)
        {
            return new TextButtonStyle
            {
                //Up = new PrimitiveDrawable(upColor),
                //Down = new PrimitiveDrawable(downColor),
                //Over = new PrimitiveDrawable(overColor)
                Up = new PrimitiveDrawable(100,20,upColor, outlineUpColor),
                Down = new PrimitiveDrawable(100,20,downColor, outlineDownColor),
                Over = new PrimitiveDrawable(100,20,overColor, outlineOverColor),
                FontColor = fontColor ?? Color.WHITE
            };
        }

        #endregion

        public new TextButtonStyle Clone()
		{
			return new TextButtonStyle
			{
				Up = Up,
				Down = Down,
				Over = Over,
				Checked = Checked,
				CheckedOver = CheckedOver,
				Disabled = Disabled,

				Font = Font,
				FontColor = FontColor,
				DownFontColor = DownFontColor,
				OverFontColor = OverFontColor,
				CheckedFontColor = CheckedFontColor,
				CheckedOverFontColor = CheckedOverFontColor,
				DisabledFontColor = DisabledFontColor,
			};
		}
	}
}