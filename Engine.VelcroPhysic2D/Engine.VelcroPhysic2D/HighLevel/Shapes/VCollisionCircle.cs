using Engine;
using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Raylib_cs;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace Engine.Velcro
{
    using MVec2 = Microsoft.Xna.Framework.Vector2;

    public class VCollisionCircle : VCollisionShape
    {
        /// <summary>
        /// In simulation unit
        /// </summary>
        private static float defaultRadius = 10  * VConvert.DisplayToSim;

        float _radius = defaultRadius;
        Vector2 _center;

        public float Radius
        {
            get => _radius;
            set => SetRadius(value);
        }

        public VCollisionCircle(float radius) : this()
        {
            _fixtureDef.Shape.Radius = radius * VConvert.DisplayToSim;
            _radius = radius;
        }

        public VCollisionCircle()
        {
            _fixtureDef.Shape = new CircleShape(defaultRadius,defaultDensitiy);
        }

        public VCollisionCircle SetRadius(float radius)
        {
            Insist.IsTrue(radius > 0, "radius must be more than zero");
            _radius = radius;
            RecreateFixture();
            return this;
        }

        public VCollisionCircle SetCenter(float x, float y)
        {
            _center.X = x;
            _center.Y = y;
            RecreateFixture();
            return this;
        }
        public VCollisionCircle SetCenter(Vector2 center)
            => SetCenter(center.X,center.Y);


        void RecreateFixture()
        {
            var simRadius = _radius * Transform.Scale.X * VConvert.DisplayToSim;
            var simCenter = _center.ToMVec2() * VConvert.DisplayToSim;

            _fixtureDef.Shape.Radius = simRadius;
            (_fixtureDef.Shape as CircleShape).Position = simCenter ;

            if(RawFixture != null)
            {
                var shape = RawFixture.Shape as CircleShape;
                shape.Radius = simRadius;
                shape.Position = simCenter;
                WakeContactBodies();
            }
        }

        internal override void DrawShape(Body body)
        {
            var origin = body.Position.ToSVec2() * VConvert.SimToDisplay  ;
            var rot = body.Rotation;

            var rotateMatix = Matrix3x2.CreateRotation(rot);

            var rotatedLocalPoint = Vector2.Transform(_center,rotateMatix);

            var pos = rotatedLocalPoint + origin;

            var ePoint = Raymath.Vector2Rotate(Vector2.UnitY * _radius, body.Rotation);
            var color = body.GetColor();
            var width = body.GetWidth();
            RayUtils.DrawCircleLines(pos,_radius ,width,color);
            Raylib.DrawLineEx(pos,pos + ePoint,width,color);
        }

        public override Component DeepClone()
        {
            return new VCollisionCircle(_radius)
            {
                _center = this._center,
            };
        }
    }
}