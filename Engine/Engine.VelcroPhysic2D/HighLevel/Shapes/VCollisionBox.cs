using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using Genbox.VelcroPhysics.Utilities;
using Raylib_cs;
using Raylib_cs.Extension;
using System.Numerics;

namespace Engine.Velcro
{
    public class VCollisionBox : VCollisionPolygon
    {
         float _width = 0.1f, _height = 0.1f;

        public Vector2 Dimension => new Vector2(Width, Height);
        public float Width
        {
            get => _width;
            set => SetSize(Width,_height);
        }
        public float Height
        {
            get => _height;
            set => SetSize(_width, Height);
        }
        #region Constructors

        public VCollisionBox(Vector2 scale) : this(scale.X, scale.Y) { } 
        public VCollisionBox(float width, float height)
        {
            _width = width;
            _height = height;
            CreateBoxVertices();
        }
        public VCollisionBox() : base()
        {
        } 

        #endregion

        public VCollisionBox SetSize(float width,float height)
        {
            _width = width;
            _height = height;
            if (_vertices == null) CreateBoxVertices();
            else RecreateFixture();
            return this;
        }


        void CreateBoxVertices()
        {
            _vertices = PolygonUtils.CreateRectangle(
                _width / 2f * VConvert.DisplayToSim,
                _height / 2f * VConvert.DisplayToSim
                );
            _vertsDirty = true;
        }

        internal override void DrawShape(Body body)
        {
            var scale = new Vector2(_width, _height) ;
            var pos = body.Position.ToSVec2() * VConvert.SimToDisplay;
            var rec = RectangleExt.CreateRectangle(pos, scale);
            var org = scale / 2f;
            var rot = body.Rotation * Raylib.RAD2DEG;
            var lineWidth = body.GetWidth();
            var color = body.GetColor();
            RayUtils.DrawRectangleLines(rec, org, rot, lineWidth, color);

            Raylib.DrawLineEx(pos, pos + Raymath.Vector2Rotate(Vector2.UnitY * scale.Y/2f, body.Rotation),lineWidth , color);
        }

        public override Component DeepClone()
        {
            var clone = new VCollisionBox(Width, Height);
            clone._fixtureDef.Shape = _fixtureDef.Shape.Clone();
            clone._fixtureDef.IsSensor = _fixtureDef.IsSensor;
            clone._fixtureDef.Filter.Category = _fixtureDef.Filter.Category;
            clone._fixtureDef.Filter.CategoryMask = _fixtureDef.Filter.CategoryMask;

            return clone;
        }


    }

}