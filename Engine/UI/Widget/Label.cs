
using Engine.BitmapFonts;
using Raylib_cs;
using Raylib_cs.UI.Extra;
using BitmapFont = Raylib_cs.Font;
using Color = Raylib_cs.Color;
using Vector2 = System.Numerics.Vector2;


namespace Engine.UI
{
	public class Label : Element 
	{
		public override float PreferredWidth
		{
			get
			{
				if (_wrapText)
					return 0;

				if (_prefSizeInvalid)
					ComputePrefSize();

				var w = _prefSize.X;
				if (_style.Background != null)
					w += _style.Background.LeftWidth + _style.Background.RightWidth;
				return w;
			}
		}

		public override float PreferredHeight
		{
			get
			{
				if (_prefSizeInvalid)
					ComputePrefSize();

				var h = _prefSize.Y;
				if (_style.Background != null)
					h += _style.Background.TopHeight + _style.Background.BottomHeight;
				return h;
			}
		}


		// configuration
		LabelStyle _style;
		string _text;

		int labelAlign = AlignInternal.Left;

		//int lineAlign = AlignInternal.left;
		string _ellipsis;
		bool _wrapText;

		// internal state
		string _wrappedString;
		bool _prefSizeInvalid;
		float _lastPrefHeight;
		Vector2 _prefSize;
		Vector2 _textPosition;



		public Label(string text, LabelStyle style)
		{
			SetStyle(style);
			SetText(text);
			touchable = Touchable.Disabled;
		}


#if false
        public Label(string text, Skin skin, string styleName = null) : this(text, skin.Get<LabelStyle>(styleName))
        { } 
#endif


        public Label(string text, BitmapFont font, Color fontColor) : this(text, new LabelStyle(font, fontColor))
		{ }


		public Label(string text, BitmapFont font, Color fontColor, float fontScale) : this(text, new LabelStyle(font, fontColor, fontScale))
        { }


		public Label(string text, BitmapFont font) : this(text, font, Color.WHITE)
		{ }


		public Label(string text) : this(text, Raylib.GetFontDefault())
		{ }

		public Label(string text,float fontScale) : this(text, Raylib.GetFontDefault(), Color.WHITE, fontScale) { }


		public virtual Label SetStyle(LabelStyle style)
		{
			_style = style;
			InvalidateHierarchy();
			return this;
		}


		/// <summary>
		/// Returns the button's style. Modifying the returned style may not have an effect until {@link #setStyle(ButtonStyle)} is called.
		/// </summary>
		/// <returns>The style.</returns>
		public virtual LabelStyle GetStyle()
		{
			return _style;
		}


		public override void Invalidate()
		{
			base.Invalidate();
			_prefSizeInvalid = true;
		}




		#region Configuration

		public Label SetText(string text)
		{
			if (_text != text)
			{
				_wrappedString = null;
				_text = text;
				_prefSizeInvalid = true;
				InvalidateHierarchy();
			}

			return this;
		}


		public string GetText()
		{
			return _text;
		}


		/// <summary>
		/// background may be null to clear the background.
		/// </summary>
		/// <returns>this</returns>
		/// <param name="background">Background.</param>
		public Label SetBackground(IDrawable background)
		{
			_style.Background = background;
			Invalidate();
			return this;
		}


		/// <summary>
		/// alignment Aligns all the text within the label (default left center) and each line of text horizontally (default left)
		/// </summary>
		/// <param name="alignment">Alignment.</param>
		public Label SetAlignment(Align alignment)
		{
			return SetAlignment(alignment, alignment);
		}

        #region Aligment Shorcut
        public Label Center() => SetAlignment(Align.Center);
        public Label TopLeft() => SetAlignment(Align.TopLeft);
        public Label TopRight() => SetAlignment(Align.TopRight);
        public Label BottomLeft() => SetAlignment(Align.BottomLeft);
        public Label BottomRight() => SetAlignment(Align.BottomRight);
        public Label Top() => SetAlignment(Align.Top);
        public Label Bottom() => SetAlignment(Align.Bottom);
        public Label Left() => SetAlignment(Align.Left);
        public Label Right() => SetAlignment(Align.Right); 
        #endregion

        /// <summary>
        /// labelAlign Aligns all the text within the label (default left center).
        /// lineAlign Aligns each line of text horizontally (default left).
        /// </summary>
        /// <param name="labelAlign">Label align.</param>
        /// <param name="lineAlign">Line align.</param>
        public Label SetAlignment(Align labelAlign, Align lineAlign)
		{
			this.labelAlign = (int)labelAlign;

			// TODO
			//			var tempLineAlign = (int)lineAlign;
			//			if( ( tempLineAlign & AlignInternal.left ) != 0 )
			//				this.lineAlign = AlignInternal.left;
			//			else if( ( tempLineAlign & AlignInternal.right ) != 0 )
			//				this.lineAlign = AlignInternal.right;
			//			else
			//				this.lineAlign = AlignInternal.center;

			Invalidate();
			return this;
		}


		public Label SetLabelFontColor(Color color)
		{
			_style.FontColor = color;
			return this;
		}


		public Label SetFontScale(float fontScale)
		{
			_style.FontScale = fontScale;
			InvalidateHierarchy();
			return this;
		}




		/// <summary>
		/// When non-null the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not occur
		/// when ellipsis is enabled. Default is null.
		/// </summary>
		/// <param name="ellipsis">Ellipsis.</param>
		public Label SetEllipsis(string ellipsis)
		{
			_ellipsis = ellipsis;
			return this;
		}


		/// <summary>
		/// When true the text will be truncated "..." if it does not fit within the width of the label. Wrapping will not occur when
		/// ellipsis is true. Default is false.
		/// </summary>
		/// <param name="ellipsis">Ellipsis.</param>
		public Label SetEllipsis(bool ellipsis)
		{
			if (ellipsis)
				_ellipsis = "...";
			else
				_ellipsis = null;
			return this;
		}


		/// <summary>
		/// should the text be wrapped?
		/// </summary>
		/// <param name="shouldWrap">If set to <c>true</c> should wrap.</param>
		public Label SetWrap(bool shouldWrap)
		{
			_wrapText = shouldWrap;
			InvalidateHierarchy();
			return this;
		}

		#endregion

		void ComputePrefSize()
		{
			_prefSizeInvalid = false;

			if (_wrapText && _ellipsis == null && width > 0)
			{
				var widthCalc = width;
				if (_style.Background != null)
					widthCalc -= _style.Background.LeftWidth + _style.Background.RightWidth;

				//TODO WARP TEXT
				//_wrappedString = _style.Font.WrapText(_text, widthCalc / _style.FontScaleX);
			}
			else if (_ellipsis != null && width > 0)
			{
				// we have a max width and an ellipsis so we will truncate the text
				var widthCalc = width; 
				if (_style.Background != null)
					widthCalc -= _style.Background.LeftWidth + _style.Background.RightWidth;

				//TODO Truncate 
			//	_wrappedString = _style.Font.TruncateText(_text, _ellipsis, widthCalc / _style.FontScaleX);
			}
			else
			{
				_wrappedString = _text;
			}

			//TODO Truncate 
			var textScale = Raylib.MeasureTextEx(_style.Font, _wrappedString, _style.FontScale,_style.Spacing);
			_prefSize = textScale;
				//_style.Font.MeasureString(_wrappedString) * new Vector2(_style.FontScaleX, _style.FontScaleY);
		}

		public override void Layout()
		{
			if (_prefSizeInvalid)
				ComputePrefSize();

			var isWrapped = _wrapText && _ellipsis == null;
			if (isWrapped)
			{
				if (_lastPrefHeight != PreferredHeight)
				{
					_lastPrefHeight = PreferredHeight;
					InvalidateHierarchy();
				}
			}

			var width = this.width;
			var height = this.height;
			_textPosition.X = 0;
			_textPosition.Y = 0;

			// TODO: explore why descent causes mis-alignment
			//_textPosition.Y =_style.font.descent;
			if (_style.Background != null)
			{
				_textPosition.X = _style.Background.LeftWidth;
				_textPosition.Y = _style.Background.TopHeight;
				width -= _style.Background.LeftWidth + _style.Background.RightWidth;
				height -= _style.Background.TopHeight + _style.Background.BottomHeight;
			}

			float textWidth, textHeight;
			if (isWrapped || _wrappedString.IndexOf('\n') != -1)
			{
				// If the text can span multiple lines, determine the text's actual size so it can be aligned within the label.
				textWidth = _prefSize.X;
				textHeight = _prefSize.Y;

				if ((labelAlign & AlignInternal.Left) == 0)
				{
					if ((labelAlign & AlignInternal.Right) != 0)
						_textPosition.X += width - textWidth;
					else
						_textPosition.X += (width - textWidth) / 2;
				}
			}
			else
			{
				textWidth = width;
				textHeight = _style.FontScale;// _style.Font.LineHeight * _style.FontScaleY;
			}
			if ((labelAlign & AlignInternal.Bottom) != 0)
			{
				_textPosition.Y += height - textHeight;
				y += _style.Padding.Bottom;
			}
			else if ((labelAlign & AlignInternal.Top) != 0)
			{
				_textPosition.Y += 0;
				y -= _style.Padding.Bottom;
			}
			else
			{
				_textPosition.Y += (height - textHeight) / 2;
			}

			//_textPosition.Y += textHeight;

			// if we have GlyphLayout this code is redundant


			if ((labelAlign & AlignInternal.Left) != 0)
				_textPosition.X = 0;
			else if ((labelAlign & AlignInternal.Right) != 0)
				_textPosition.X = width - _prefSize.X; // full width - our text size
			else if (labelAlign == AlignInternal.Center 
				|| (((labelAlign & AlignInternal.Top) | (labelAlign & AlignInternal.Bottom)) != 0))
				_textPosition.X = (width - _prefSize.X) / 2; // center of width - center of text size
			else
				_textPosition.X = width - _prefSize.X; // full width - our text size
		}

		public override void Draw( float parentAlpha)
		{
			Validate();

			var color = Raylib.ColorAlpha(this.color, (int)(this.color.a * parentAlpha));
			_style.Background?.Draw( x, y, width == 0 ? _prefSize.X : width, height, color);

			Raylib.DrawTextPro(_style.Font,_wrappedString,new Vector2(x,y) + _textPosition,Vector2.Zero,rotation,_style.FontScale,_style.Spacing,_style.FontColor);

			//batcher.DrawString(_style.Font, _wrappedString, new Vector2(x, y) + _textPosition,
				//_style.FontColor, 0, Vector2.Zero, new Vector2(_style.FontScaleX, _style.FontScaleY), SpriteEffects.None, 0);
		}
	}


	/// <summary>
	/// the style for a label
	/// </summary>
	public class LabelStyle
	{
		public Color FontColor = Color.WHITE;
		public BitmapFont Font = Raylib.GetFontDefault();
		public Padding Padding;
		public float Spacing = 1;
		public IDrawable Background;
		public float FontScale = 10f;

		public LabelStyle() { }

		public LabelStyle(BitmapFont font, Color fontColor, float fontScale = 20f, float spacing = 1f)
		{
			var defaultFont = Raylib_cs.Raylib.GetFontDefault();
			Font = font.texture.id != 0 ? font : defaultFont;
			FontColor = fontColor;
			FontScale = fontScale;
			Spacing = spacing;
		}




		public LabelStyle(Color fontColor) : this(Raylib.GetFontDefault(), fontColor)
		{ }


		public LabelStyle Clone()
		{
			return new LabelStyle
			{
				FontColor = FontColor,
				Font = Font,
				Background = Background,
				FontScale = FontScale,
			};
		}
	}
}