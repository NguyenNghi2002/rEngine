﻿#if true
using System;
using Vector2 = System.Numerics.Vector2;
using Color = Raylib_cs.Color;
using Raylib_cs;

namespace Engine.UI
{
    public class Slider : ProgressBar, IInputListener, IGamepadFocusable
    {
        /// <summary>
        /// the maximum distance outside the slider the mouse can move when pressing it to cause it to be unfocused
        /// </summary>
        public float SliderBoundaryThreshold = 50f;

        SliderStyle style;
        bool _mouseOver, _mouseDown;


        /// <summary>
        /// Creates a new slider. It's width is determined by the given prefWidth parameter, its height is determined by the maximum of
        ///  the height of either the slider {@link NinePatch} or slider handle {@link TextureRegion}. The min and max values determine
        /// the range the values of this slider can take on, the stepSize parameter specifies the distance between individual values.
        /// E.g. min could be 4, max could be 10 and stepSize could be 0.2, giving you a total of 30 values, 4.0 4.2, 4.4 and so on.
        /// </summary>
        /// <param name="min">Minimum.</param>
        /// <param name="max">Max.</param>
        /// <param name="stepSize">Step size.</param>
        /// <param name="vertical">If set to <c>true</c> vertical.</param>
        /// <param name="background">Background.</param>
        public Slider(float min, float max, float stepSize, bool vertical, SliderStyle style) : base(min, max, stepSize,
            vertical, style)
        {
            ShiftIgnoresSnap = true;
            this.style = style;
        }

#if true
        public Slider(float min, float max, float stepSize, bool vertical, Skin skin, string styleName = null) : this(
            min, max, stepSize, vertical, skin.Get<SliderStyle>(styleName))
        {
        }

        public Slider(Skin skin, string styleName = null) : this(0, 1, 0.1f, false, skin.Get<SliderStyle>(styleName))
        {
        } 

        // Leaving this constructor for backwards-compatibility
        public Slider(Skin skin, string styleName = null, float min = 0, float max = 1, float step = 0.1f) : this(min,
            max, step, false, skin.Get<SliderStyle>(styleName))
        {
        }
#endif

        #region IInputListener

        void IInputListener.OnMouseEnter()
        {
            _mouseOver = true;
        }


        void IInputListener.OnMouseExit()
        {
            _mouseOver = _mouseDown = false;
        }


        bool IInputListener.OnLeftMousePressed(Vector2 mousePos)
        {
            CalculatePositionAndValue(mousePos);
            _mouseDown = true;
            return true;
        }


        bool IInputListener.OnRightMousePressed(Vector2 mousePos)
        {
            return false;
        }


        void IInputListener.OnMouseMoved(Vector2 mousePos)
        {
            if (DistanceOutsideBoundsToPoint(mousePos) > SliderBoundaryThreshold)
            {
                _mouseDown = _mouseOver = false;
                GetStage().RemoveInputFocusListener(this);
            }
            else
            {
                CalculatePositionAndValue(mousePos);
            }
        }


        void IInputListener.OnLeftMouseUp(Vector2 mousePos)
        {
            _mouseDown = false;
        }


        void IInputListener.OnRightMouseUp(Vector2 mousePos)
        {
            _mouseDown = false;
        }


        bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
        {
            return false;
        }

        #endregion


        #region IGamepadFocusable

        public bool ShouldUseExplicitFocusableControl { get; set; }
        public IGamepadFocusable GamepadUpElement { get; set; }
        public IGamepadFocusable GamepadDownElement { get; set; }
        public IGamepadFocusable GamepadLeftElement { get; set; }
        public IGamepadFocusable GamepadRightElement { get; set; }


        public void EnableExplicitFocusableControl(IGamepadFocusable upEle, IGamepadFocusable downEle,
                                                   IGamepadFocusable leftEle, IGamepadFocusable rightEle)
        {
            ShouldUseExplicitFocusableControl = true;
            GamepadUpElement = upEle;
            GamepadDownElement = downEle;
            GamepadLeftElement = leftEle;
            GamepadRightElement = rightEle;
        }


        void IGamepadFocusable.OnUnhandledDirectionPressed(Direction direction)
        {
            OnUnhandledDirectionPressed(direction);
        }


        void IGamepadFocusable.OnFocused()
        {
            OnFocused();
        }


        void IGamepadFocusable.OnUnfocused()
        {
            OnUnfocused();
        }


        void IGamepadFocusable.OnActionButtonPressed()
        {
            OnActionButtonPressed();
        }


        void IGamepadFocusable.OnActionButtonReleased()
        {
            OnActionButtonReleased();
        }

        #endregion


        #region overrideable focus handlers

        protected virtual void OnUnhandledDirectionPressed(Direction direction)
        {
            if (direction == Direction.Up || direction == Direction.Right)
                SetValue(_value + StepSize);
            else
                SetValue(_value - StepSize);
        }


        protected virtual void OnFocused()
        {
            _mouseOver = true;
        }


        protected virtual void OnUnfocused()
        {
            _mouseOver = _mouseDown = false;
        }


        protected virtual void OnActionButtonPressed()
        {
            _mouseDown = true;
        }


        protected virtual void OnActionButtonReleased()
        {
            _mouseDown = false;
        }

        #endregion


        public Slider SetStyle(SliderStyle style)
        {
            Insist.IsTrue(style is SliderStyle, "style must be a SliderStyle");

            base.SetStyle(style);
            this.style = style;
            return this;
        }


        /// <summary>
        /// Returns the slider's style. Modifying the returned style may not have an effect until {@link #setStyle(SliderStyle)} is called
        /// </summary>
        /// <returns>The style.</returns>
        public new SliderStyle GetStyle()
        {
            return style;
        }


        public bool IsDragging()
        {
            return _mouseDown && _mouseOver;
        }


        protected override IDrawable GetKnobDrawable()
        {
            if (Disabled && style.DisabledKnob != null)
                return style.DisabledKnob;

            if (IsDragging() && style.KnobDown != null)
                return style.KnobDown;

            if (_mouseOver && style.KnobOver != null)
                return style.KnobOver;

            return style.Knob;
        }


        void CalculatePositionAndValue(Vector2 mousePos)
        {
            var knob = GetKnobDrawable();

            float value;
            if (_vertical)
            {
                var height = this.height - style.Background.TopHeight - style.Background.BottomHeight;
                var knobHeight = knob == null ? 0 : knob.MinHeight;
                position = mousePos.Y - style.Background.BottomHeight - knobHeight * 0.5f;
                value = Min + (Max - Min) * (position / (height - knobHeight));
                position = Math.Max(0, position);
                position = Math.Min(height - knobHeight, position);
            }
            else
            {
                var width = this.width - style.Background.LeftWidth - style.Background.RightWidth;
                var knobWidth = knob == null ? 0 : knob.MinWidth;
                position = mousePos.X - style.Background.LeftWidth - knobWidth * 0.5f;
                value = Min + (Max - Min) * (position / (width - knobWidth));
                position = Math.Max(0, position);
                position = Math.Min(width - knobWidth, position);
            }

            SetValue(value);
        }
    }


    public class SliderStyle : ProgressBarStyle
    {
        /** Optional. */
        public IDrawable KnobOver, KnobDown;


        public SliderStyle()
        {
        }


        public SliderStyle(IDrawable background, IDrawable knob) : base(background, knob)
        {
        }

        public static SliderStyle CreateRaylib()
        {
            //return Create(Color.RED, Color.PINK, Color.GRAY, Color.WHITE);
            //return Create(
            //  UIDefault.BaseUp,UIDefault.OutlineUp,UIDefault.BaseDown,UIDefault.OutlineDown) ;
            return Create(UIDefault.BaseDown, UIDefault.OutlineDown, UIDefault.BaseUp, UIDefault.OutlineUp);
        }
        public static SliderStyle Create(Color beforeKnob, Color beforeKnobBorder, Color afterKnob,Color afterKnobBorder)
        {
            var bg = new PrimitiveDrawable(beforeKnob,beforeKnobBorder);
            bg.MinWidth = bg.MinHeight = 20;

            var after = new PrimitiveDrawable(afterKnob,afterKnobBorder);
            after.MinWidth = after.MinHeight = 20;

            return new SliderStyle
            {
                KnobAfter = after,
                Background = bg,
            };
        }
        public static SliderStyle CreateWithKnob(
            Color background,Color backgroundOut,
            Color beforeKnob,Color outlineBefore,
            Color afterKnob, Color outlineAfter,
            Color knobColor)
        {

            var bg = new PrimitiveDrawable(background,backgroundOut);
            bg.MinWidth = bg.MinHeight = 20;
            var l = bg.LineWidth;


            var before = new PrimitiveDrawable(beforeKnob,outlineBefore);
            before.MinWidth = before.MinHeight = 15;

            var after = new PrimitiveDrawable(afterKnob,outlineAfter);
            after.MinWidth = after.MinHeight = 15;

            var knob = new PrimitiveDrawable(knobColor);
            knob.MinWidth = 15;
            knob.MinHeight=15;


            return new SliderStyle
            {
                Knob = knob,
                KnobAfter = after,
                KnobBefore = before,
                Background = bg
            };
        }


        public new SliderStyle Clone()
        {
            return new SliderStyle
            {
                Background = Background,
                DisabledBackground = DisabledBackground,
                Knob = Knob,
                DisabledKnob = DisabledKnob,
                KnobBefore = KnobBefore,
                KnobAfter = KnobAfter,
                DisabledKnobBefore = DisabledKnobBefore,
                DisabledKnobAfter = DisabledKnobAfter,

                KnobOver = KnobOver,
                KnobDown = KnobDown
            };
        }
    }
} 
#endif