using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Shared;
using System.Numerics;
using MVec2 = Microsoft.Xna.Framework.Vector2;

namespace Engine.Velcro
{
    public class VCollisionPolygon : VCollisionShape
    {
        protected Vertices _vertices;

        Vector2 _origin;
        protected bool _vertsDirty = true;


        public VCollisionPolygon()
        {
            _fixtureDef.Shape = new PolygonShape(defaultDensitiy) ;
        }
        public VCollisionPolygon(Vertices vertices) : this()
        {
            this._vertices = vertices;
            _vertices.Scale(new MVec2(VConvert.DisplayToSim));
        }
        public VCollisionPolygon(List<Vector2> vertices) 
            : this(new Vertices(vertices.Select(svec => svec.ToMVec2())))
        {}

        public override void OnAddedToEntity()
        {
            UpdateVertsDef();
            CreateFixture();
        }


        /// <summary>
        /// <see cref="VCollisionPolygon._vertices"/> will be filled ( not null )
        /// </summary>
        internal override void CreateFixture()
        {
            UpdateVertsDef();
            base.CreateFixture();
        }

        protected void RecreateFixture()
        {
            DestroyFixture();
            UpdateVertsDef();
            CreateFixture();
        }
        /// <summary>
        /// Update Shape in fixture if available, 
        /// else update in fixture definition
        /// </summary>
        protected void UpdateVertsDef()
        {
            Insist.IsNotNull(_vertices, "Vertices cannot be null!");

            if (!_vertsDirty) return;

            _vertsDirty = false;

            var shape = (_fixtureDef.Shape as PolygonShape);

            shape.Vertices = _vertices;
            shape.Vertices.Scale(Transform.Scale.ToMVec2());
            shape.Vertices.Translate(_origin.ToMVec2());

            (_fixtureDef.Shape as PolygonShape).Vertices = _vertices;
        }

        internal override void DrawShape(Body body)
        {
            var simOrigin = body.Position.ToSVec2() ;
            var n = _vertices.Count;
            var color = body.GetColor();
            var lineWidth = body.GetWidth();
            VSimulationDebugDraw.DrawPoly(this.RawFixture.Shape as PolygonShape,body,VDebugDrawInfo.lineWidth,color);
#if false
            for (int i = 0; i < n; i++)
            {

                var v0 = Raymath.Vector2Rotate(_vertices[i].ToSVec2(), body.Rotation) + simOrigin;
                var v1 = Raymath.Vector2Rotate(_vertices[(i + 1) % n].ToSVec2(), body.Rotation) + simOrigin;

                v0 *= VConvert.SimToDisplay;
                v1 *= VConvert.SimToDisplay;

                Raylib.DrawLineEx(v0, v1, lineWidth, color);
                //Raylib.DrawCircleV(v0, 5, Color.YELLOW);
            } 
#endif
        }

    }
}