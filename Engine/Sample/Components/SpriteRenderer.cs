using Engine.Texturepacker;
using Raylib_cs;
using Raylib_cs.Extension;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Numerics;


namespace Engine
{
    public class ScrollingSpriteRenderer : TiledSpriteRenderer,IUpdatable
    {
        public ScrollingSpriteRenderer(Texture2D texture,float scrollXSpeed = 15, float scrollYSpeed = 5) : base(texture)
        {
            this.ScrollSpeedX = scrollXSpeed;
            this.ScrollSpeedY = scrollYSpeed;
        }

        public int UpdateOrder { get; set; }

        public float ScrollSpeedX = 15;
        public float ScrollSpeedY = 5;


        public void Update()
        {
            if(ScrollSpeedX != 0) SourceX -= ScrollSpeedX * Time.DeltaTime;
            if(ScrollSpeedY != 0) SourceY += ScrollSpeedY * Time.DeltaTime;
        }
    }
    public class TiledSpriteRenderer : SpriteRenderer
    {
        public float SourceX
        {
            get => _sourceRec.x;
            set => _sourceRec.x = value;
        }
        public float SourceY
        {
            get => _sourceRec.y;
            set => _sourceRec.y = value;
        }
        public float SourceWidth
        {
            get => _sourceRec.width;
            set => _sourceRec.width = value;
        }
        public float SourceHeight
        {
            get => _sourceRec.height;
            set => _sourceRec.height = value;
        }

        Rectangle _sourceRec;
        public TiledSpriteRenderer(Texture2D texture) : base(texture)
        {
            _sourceRec = Sprite.SourceRec;
        }

        public override void Render()
        {
            UpdateDrawInfo();
            var degree = Entity.Transform.EulerRotation.Z * Raylib.RAD2DEG;
            var texture = Sprite.Atlas.Texture.Value;
            if (Sprite != null && texture.id != 0)
            {
                Raylib.DrawTexturePro(texture, _sourceRec, destination, Origin, degree, TintColor);
            }
            else
            {
                Raylib.DrawTextPro(rFont.Default, $"[{Entity.Name}]SpriteRenderer", Transform.Position.ToVec2(), default, 0, 20, 2, TintColor);
            }
        }
    }
    public class SpriteRenderer : RenderableComponent,ICustomInspectorImgui
    {
        /// <summary>
        /// scale before attach to entity
        /// </summary>

        public Color TintColor
        {
            get => _tintColor;
            set => SetTintColor(value);
        }
        public Sprite Sprite
        {
            get => _sprite;
            set => SetSprite(value);
        }

        public bool FlipX
        {
            get =>_flippedX;
            set
            {
                _flippedX = value;
                destinationDirty = true;
            }
        }

        public bool FlipY
        {
            get => _flippedY;
            set
            {
                _flippedY = value;
                destinationDirty = true;
            }
        }


        /// <summary>
        /// Origin default by half of destination scale
        /// </summary>
        public Vector2 Origin
        {
            get => _origin;
        }


        protected Rectangle source, destination;
        bool _flippedX, _flippedY ;
        Sprite _sprite;
        Vector2 _origin;
        Vector4 _tintColorNormal;
        Color _tintColor;

        bool destinationDirty = true;

        public SpriteRenderer SetSprite(Sprite sprite)
        {
            this._sprite = sprite;
            destinationDirty = true;
            return this;
        }
        public SpriteRenderer SetTintColor(Vector4 tintColor)
        {
            this._tintColor = Raylib.ColorFromNormalized(tintColor);
            _tintColorNormal = tintColor ;
            return this;
        }
        public SpriteRenderer SetTintColor(Color tintColor)
        {
            this._tintColor = tintColor;
            _tintColorNormal = Raylib.ColorNormalize(tintColor);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="syncTransform">transform</param>
        public SpriteRenderer(Texture2D texture):this(new Sprite(texture)) { }
        public SpriteRenderer(Sprite sprite)
        {
            SetSprite(sprite);
            SetTintColor(Color.WHITE);

            Debug.Assert(_sprite != null);
        }

        public override void OnTransformChanged(Transformation.Component component)
        {
            destinationDirty = true;
        }
        protected void UpdateDrawInfo()
        {
            if (_sprite == null || Entity == null || !destinationDirty) return;

            ///Calculate Destination rectangle
            var scaled = _sprite.SourceScale * Entity.Transform.Scale.ToVec2();

            destination = RectangleExt.CreateRectangle(Entity.Transform.Position.ToVec2(), scaled);
            _origin = scaled / 2f;
            destinationDirty = false;

            ///Calculate Source rectangle
            source = Sprite.SourceRec;
            source.width = source.width * (_flippedX ? -1 : 1);
            source.height = source.height * (_flippedY ? -1 : 1);
        }
        
        public override void Render()
        {
            UpdateDrawInfo();
            var degree = Entity.Transform.EulerRotation.Z * Raylib.RAD2DEG;
            var texture = _sprite.Atlas.Texture.Value;
            if (Sprite != null && texture.id != 0)
            {
                Raylib.DrawTexturePro(texture, source, destination, Origin, degree, TintColor);
            }
            else
            {
                Raylib.DrawTextPro(rFont.Default, $"[{Entity.Name}]SpriteRenderer", Transform.Position.ToVec2(), default, 0, 20, 2, TintColor);
            }
        }

#if false
        public override void DrawDebug()
        {
            if (Transfrom.Scale.X == 0 && Transfrom.Scale.Y == 0)
            {
                Raylib.DrawCircleV(Transfrom.Position, 2f, Color.RED);
                Raylib.DrawCircleSectorLines(Transfrom.Position, 9f, 0, 360, 0, Color.RED);

            }


            var rot = Transfrom.Rotation * Raylib.DEG2RAD;
            Vector2[] ver = new Vector2[5]
            {
                RaymathF.Vector2Rotate(Transfrom.Position,_offsetedDes.TopLeft(),rot),
                RaymathF.Vector2Rotate(Transfrom.Position,_offsetedDes.TopRight(),rot),
                RaymathF.Vector2Rotate(Transfrom.Position,_offsetedDes.BotRight(),rot),
                RaymathF.Vector2Rotate(Transfrom.Position,_offsetedDes.BotLeft(),rot),
                RaymathF.Vector2Rotate(Transfrom.Position,_offsetedDes.TopLeft(),rot),
            };

            var rec = ver.GetBound();

            Rlgl.rlSetLineWidth(2f);
            Raylib.DrawLineStrip(ver, ver.Length, Color.RED);
            Raylib.DrawRectangleLinesEx(rec, 2f, Color.YELLOW);

            //Raylib.DrawRectangleLinesEx(_offsetedDes,2 / Entity.Scene.Camera.zoom,Color.GREEN);
        } 
#endif

#if true
        void ICustomInspectorImgui.OnInspectorGUI()
        {
            
            var flags = ImGuiNET.ImGuiColorEditFlags.AlphaBar
                | ImGuiNET.ImGuiColorEditFlags.AlphaPreviewHalf
                | ImGuiNET.ImGuiColorEditFlags.DefaultOptions;
            if(_sprite != null)
            {
                var txt = _sprite.Atlas.Texture.Value;
                if (ImGuiNET.ImGui.ColorEdit4("Tint Color", ref _tintColorNormal, flags))
                {
                    TintColor = Raylib.ColorFromNormalized(_tintColorNormal);
                }
                ImGuiNET.ImGui.Separator();
                ImGuiNET.ImGui.TextColored(_tintColorNormal, "Texture");
                ImGuiNET.ImGui.Image((IntPtr)txt.id, txt.Scale(), _sprite.SourceRec.TopLeft() / txt.Scale(), _sprite.SourceRec.BotRight() / txt.Scale(), _tintColorNormal);
                ImGuiNET.ImGui.SameLine();

                
                ImGuiNET.ImGui.Text(txt.height.ToString());
                ImGuiNET.ImGui.Text(txt.width.ToString());
            }
        } 
#endif

        public override Component DeepClone()
        {
            var clone = new SpriteRenderer(_sprite)
            {
                TintColor = this.TintColor,
            };
            return clone;
        }

    }


}