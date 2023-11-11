using Engine;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Engine.Velcro.Unit;
using Raylib_cs;
using Genbox.VelcroPhysics.Definitions;
using Genbox.VelcroPhysics.Collision.Handlers;
using System.Numerics;
using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Extensions.DebugView;
using MVec2 = Microsoft.Xna.Framework.Vector2;
using Genbox.VelcroPhysics.Utilities;
using ImGuiNET;

namespace Engine.Velcro
{
    /// <summary>
    /// collider for <see cref="VRigidBody2D"/>
    /// </summary>
    public interface ICollidable
    {
        void CreateCollider();
        void DrawCollider();
        void DestroyCollider();
    }

    public class VRigidBody2D : Component,IUpdatable,ICustomInspectorImgui
    {
        /// <summary>
        /// Created in OnAddedToEntity
        /// </summary>
        public Body Body ;
        public Vector2 Position
        {
            get =>Body != null ? Body.Position * VConvert.SimToDisplay : Transform.Position2;
            set
            {
                if (Body != null)
                    Body.Position = value.ToMVec2() * VConvert.DisplayToSim;
                else
                    Transform.Position2 = value;
            }
        }
        public Vector2 Velocity
        {
            get => (Body != null ? Body.LinearVelocity : _bodyDef.LinearVelocity) * VConvert.SimToDisplay;
            set => SetVelocity(value);
        }

        BodyDef _bodyDef = new BodyDef();

        /// <summary>
        /// Prevent recurrsive happen in OnTransfromChanged
        /// </summary>
        bool _ignoreTransformChanges;

        public int UpdateOrder { get; set; } = 0;
        

        #region Component Overrides
        public override void OnTransformChanged(Transformation.Component comp)
        {
            if (Body == null || _ignoreTransformChanges) return;

            if (comp == Transformation.Component.Rotation)
            {
                Body.Rotation = Transform.EulerRotation.Z;
                Body.AngularVelocity = 0;
                Body.Awake = true;
            }
            else if (comp == Transformation.Component.Position)
            {
                Body.Position = Transform.Position.ToMVec2() * VConvert.DisplayToSim;
                Body.LinearVelocity = default;
                Body.Awake = true;
            }
        }
        public override void OnAddedToEntity()
        {
            CreateBody();
        }
        public override void OnRemovedFromEntity()
        {
            DestroyBody();
        }
        public override void OnEnable()
        {
            Body.Enabled = true;
        }
        public override void OnDisable()
        {
            Body.Enabled= false;

        }
        public override void OnDebugRender()
        {
            if (Body != null )
            {
                /// Draw collision shape
                foreach (Fixture fixture in Body.FixtureList)
                {
                    VCollisionShape collider = ((Entity)fixture.UserData).GetComponent<VCollisionShape>() ;
                    if ( collider != null)
                        collider.DrawShape(Body);
                }

                ///Draw Center
                Raylib.DrawCircleV(VConvert.SimToDisplay * Body.Position.ToSVec2(), VDebugDrawInfo.centerReadius, VDebugDrawInfo.centerColor);

                /// Draw Contact Point / Contact Normal
                var current = Body.ContactList;



                while (current != null)
                {
                    current.Contact.GetWorldManifold(out var n, out var cps);
                    var cn = n.ToSVec2();

                    ///Contact point 0
                    var cp0 = cps[0].ToSVec2() * VConvert.SimToDisplay;
                    Raylib.DrawCircleV(cp0, VDebugDrawInfo.contactPoint0Radius, VDebugDrawInfo.contactPoint0Color);
                    Raylib.DrawLineEx(cp0,
                        cp0 + cn * VDebugDrawInfo.contactNormal0Length,
                        VDebugDrawInfo.contactNormal0LineWidth,
                        VDebugDrawInfo.contactNormal0Color);

                    ///Contact point 1
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

        #region Configurations
        public VRigidBody2D SetVelocity(Vector2 velocity)
        {
            var v = velocity.ToMVec2() * VConvert.DisplayToSim;
            if (Body != null)
                Body.LinearVelocity = v;
            _bodyDef.LinearVelocity= v;
            return this;
        }
        public VRigidBody2D SetFixedRotation(bool isFixedRotation)
        {
            if (Body != null)
                Body.FixedRotation = isFixedRotation;
            _bodyDef.FixedRotation = isFixedRotation;
            return this;
        }
        public VRigidBody2D SetAllowSleep(bool allowSleep)
        {
            if (Body != null) 
                Body.SleepingAllowed = allowSleep;
            _bodyDef.AllowSleep = allowSleep;
            return this;
        }
        public VRigidBody2D SetAngularDamping(float angularDamping)
        {
            if (Body != null)
                Body.AngularDamping = angularDamping;
            _bodyDef.AngularDamping = angularDamping;
            return this;
        }
        public VRigidBody2D SetBodyType(BodyType bodyType)
        {
            if (Body != null)
                Body.BodyType = bodyType;
            _bodyDef.Type = bodyType;
            return this;
        }
        public VRigidBody2D SetCollideWith(Category category)
        {
            Body.CollidesWith = category;
            return this;
        }
        public VRigidBody2D SetCollideGroup(Category category)
        {
            Body.CollisionCategories = category;
            return this;
        }

        #endregion

        public VRigidBody2D ApplyLinearImpulse(Vector2 impulse)
        {
            Body.ApplyLinearImpulse(impulse.ToMVec2() * VConvert.DisplayToSim);

            return this;
        }
        public VRigidBody2D ApplyLinearImpulse(Vector2 impulse,Vector2 point)
        {
            Body.ApplyLinearImpulse(impulse.ToMVec2() * VConvert.DisplayToSim,point.ToMVec2() * VConvert.DisplayToSim);
            return this;

        }
        public VRigidBody2D ApplyForce(Vector2 force)
        {
            Body.ApplyForce(force.ToMVec2() * VConvert.DisplayToSim);
            return this;
        }
        public VRigidBody2D ApplyForce(Vector2 force,Vector2 point)
        {

            Body.ApplyForce(force.ToMVec2() * VConvert.DisplayToSim,point.ToMVec2() * VConvert.DisplayToSim);
            return this;
        }

        void CreateBody()
        {
            if (Body != null)
                return;

            var simPos = Transform.Position.ToMVec2() * VConvert.DisplayToSim;
            var simRot = Transform.EulerRotation.Z;
            var world = Scene.GetOrCreateSceneComponent<VWorld2D>().World;

            _bodyDef.Position = simPos;
            _bodyDef.UserData = this;
            _bodyDef.Angle = simRot;

            Body  = BodyFactory.CreateFromDef(world, _bodyDef) ;

            var collisionShapes = Entity.GetComponents<VCollisionShape>();
            if (collisionShapes == null) return;

            for (int i = 0; i < collisionShapes.Count; i++)
                collisionShapes[i].CreateFixture();
            //Pool<List<VCollisionShape>>.Free(collisionShapes) ;

            //TODO: Loop create joints list
        }

        void DestroyBody()
        {
            //TODO: Loop destroy joints list

            var collisionShapes = Entity.GetComponents<VCollisionShape>();
            for (int i = 0; i < collisionShapes.Count; i++)
            {
                collisionShapes[i].DestroyFixture();
            }
            Pool<List<VCollisionShape>>.Free(collisionShapes);

            Body.RemoveFromWorld();
            Body.OnCollision = null;
            Body.OnSeparation = null;
            Body.UserData = null;
            Body = null;
        }

        public void Update()
        {
            if (Body == null )
                return;

            _ignoreTransformChanges = true;
            Transform.Position = VConvert.SimToDisplay * Body.Position.ToSVec3();
            Transform.SetRotation(Vector3.UnitZ,Body.Rotation);
            _ignoreTransformChanges = false;

        }

         public override Component DeepClone()
        {
            return new VRigidBody2D()
            {
                _bodyDef = new BodyDef()
                {
                    Type = _bodyDef.Type,
                },
            };
        }

        void ICustomInspectorImgui.OnInspectorGUI()
        {
            var v = Velocity;
            ImGui.InputFloat2("velocity",ref v,null,ImGuiInputTextFlags.ReadOnly);
        }
    }



}