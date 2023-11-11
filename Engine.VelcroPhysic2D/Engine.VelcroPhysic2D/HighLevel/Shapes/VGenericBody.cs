using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Genbox.VelcroPhysics.Tools.Cutting;
using Genbox.VelcroPhysics.Tools.Cutting.Simple;
using Genbox.VelcroPhysics.Tools.PolygonManipulation;
using Genbox.VelcroPhysics.Tools.Triangulation.Delaunay.Util;
using Genbox.VelcroPhysics.Tools.Triangulation.Earclip;
using Raylib_cs;
using System.Collections;
using System.Numerics;
using System.Threading.Tasks.Dataflow;

namespace Engine.Velcro
{
    public class VGenericBody :Component , IUpdatable
    {
        BodyDef _bodyDef = new BodyDef();
        public Body Body { get; private set; }
        public int UpdateOrder { get; set; }
        bool _ignoreTransformChanges;

        public VGenericBody()
        {
        }
        public VGenericBody(Body body) 
        {
            this.Body = body;
        }

        public void Update()
        {
            if (Body == null) return;

            _ignoreTransformChanges = true;
            Transform.SetPosition(VConvert.SimToDisplay * Body.Position.ToSVec3());
            Transform.SetRotation(Vector3.UnitZ,Body.Rotation) ;
            _ignoreTransformChanges = false;
        }



        #region Component Events
        public override void OnAddedToEntity()
        {

            if (Body != null)
            {
                this.Update();
            }
            else
            {
                var world = Scene.GetOrCreateSceneComponent<VWorld2D>();
                var pos = Transform.Position.ToMVec2() * VConvert.DisplayToSim;
                var rot = Transform.EulerRotation.Z;
                Body = BodyFactory.CreateFromDef(world, _bodyDef);

                Body.Position = pos;
                Body.Rotation = rot;
                Body.UserData = this.Entity;
            }

        }
        public override void OnRemovedFromEntity()
        {
            if (Body != null)
            {
                Body.RemoveFromWorld();
                Body = null;
            }
        }
        public override void OnTransformChanged(Transformation.Component component)
        {
            if (Body == null || _ignoreTransformChanges)
                return;

            if (component == Transformation.Component.Rotation)
            {
                Body.Rotation = Transform.EulerRotation.Z;
                Body.AngularVelocity = 0;
                Body.Awake = true;
            }
            else if (component == Transformation.Component.Position)
            {
                Body.Position = Transform.Position.ToMVec2() * VConvert.DisplayToSim;
                Body.LinearVelocity = default;
                Body.Awake = true;
            }
        } 
        public override void OnDebugRender()
        {
            if (Body != null)
            {
                /// Draw collision shape
                foreach (var fixture in Body.FixtureList)
                {
                    Shape collider = fixture.Shape;
                    switch (collider.ShapeType)
                    {
                        case ShapeType.Unknown:
                            break;
                        case ShapeType.Circle:
                            VSimulationDebugDraw.DrawCircle(fixture.Shape as CircleShape,fixture.Body,Body.GetWidth(),Body.GetColor());
                            break;
                        case ShapeType.Edge:
                            break;
                        case ShapeType.Polygon:
                            VSimulationDebugDraw.DrawPoly(fixture.Shape as PolygonShape,fixture.Body, Body.GetWidth(), Body.GetColor());
                            break;
                        case ShapeType.Chain:
                            break;
                        case ShapeType.TypeCount:
                            break;
                    }
                }

                ///Draw Center
                Raylib.DrawCircleV(VConvert.SimToDisplay * Body.GetPosition(), VDebugDrawInfo.centerReadius, VDebugDrawInfo.centerColor);

                /// Draw Contact Point / Contact Normal
                var current = Body.ContactList;
                while (current != null)
                {
                    current.Contact.GetWorldManifold(out var n, out var cps);
                    var cn = n.ToSVec2();

                    var cp0 = cps[0].ToSVec2() * VConvert.SimToDisplay;
                    Raylib.DrawCircleV(cp0, VDebugDrawInfo.contactPoint0Radius, VDebugDrawInfo.contactPoint0Color);
                    Raylib.DrawLineEx(cp0,
                        cp0 + cn * VDebugDrawInfo.contactNormal0Length,
                        VDebugDrawInfo.contactNormal0LineWidth,
                        VDebugDrawInfo.contactNormal0Color);

                    var cp1 = cps[1].ToSVec2() * VConvert.SimToDisplay;
                    Raylib.DrawCircleV(cp1,
                        VDebugDrawInfo.contactPoint1Radius,
                        VDebugDrawInfo.contactPoint1Color);
                    Raylib.DrawLineEx(cp1,
                        cp1 + cn * VDebugDrawInfo.contactNormal1Length,
                        VDebugDrawInfo.contactNormal1LineWidth,
                        VDebugDrawInfo.contactNormal1Color);

                    current = current.Next;
                }
            }
        }
        #endregion


        public static implicit operator Body (VGenericBody self)
        {
            return self.Body;
        }

        #region Configurations
        public VGenericBody SetAllowSleep(bool allowSleep)
        {
            if (Body != null)
                Body.SleepingAllowed = allowSleep;
            _bodyDef.AllowSleep = allowSleep;
            return this;
        }
        public VGenericBody SetBodyType(BodyType bodyType)
        {
            if (Body != null)
                Body.BodyType = bodyType;
            _bodyDef.Type = bodyType;
            return this;
        }
        public VGenericBody SetCollideWith(Category category)
        {
            Body.CollidesWith = category;
            return this;
        }
        public VGenericBody SetCollideGroup(Category category)
        {
            Body.CollisionCategories = category;
            return this;
        }

        #endregion

    }
}
