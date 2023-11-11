using Engine;
using Engine.SceneManager;
using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Dynamics.Joints;
using Genbox.VelcroPhysics.Utilities;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.Velcro
{
    /// <summary>
    /// Scene component
    /// </summary>
    public class VWorld2D : SceneComponent
    {

        public World World { get; private set; } = new World(0, 0.2f);
        public override void FixedUpdate()
        {
            World.Step(Time.FixedDeltaTime);
        }
        public override void OnDisabled()
        {
            World.Enabled = this.Enable;
        }
        public override void OnEnabled()
        {
            World.Enabled = this.Enable;
        }
        public override void OnAddedToScene()
        {
            World.BodyRemoved += World_OnBodyRemoved;
            World.FixtureRemoved += World_FixtureRemoved;
            World.JointRemoved += World_JointRemoved;
        }
        public override void OnRemoveFromScene()
        {
            World.ClearForces();
            World.Clear();

            World.BodyRemoved -= World_OnBodyRemoved;
            World.FixtureRemoved -= World_FixtureRemoved;
            World.JointRemoved -= World_JointRemoved;
        }

        void World_JointRemoved(Joint joint)
        {
            joint.UserData = null;
        }
        void World_FixtureRemoved(Fixture fixture)
        {
            fixture.UserData = null;
        }
        void World_OnBodyRemoved(Body body)
        {
            body.UserData = null;
        }

        public static implicit operator World(VWorld2D self)
        {
            return self.World;
        }
        ~VWorld2D()
        {
            World = null;
        }

        public bool RayCast(Func<Fixture,Vector2,Vector2,float,float> callback, Vector2 origin, Vector2 direction, float rayLength,out HitInfo hit)
        {
            var info = new HitInfo();
            var hitted = false;
            this.World.RayCast((fix, cp, cn, fraction) =>
            {
                info.fraction = callback.Invoke(fix,cp* VConvert.SimToDisplay, cn * VConvert.SimToDisplay,fraction);
                info.ContactPoint = cp * VConvert.SimToDisplay;
                info.normal = cn;
                info.distance = fraction * rayLength;
                info.fixture = fix;
                info.fraction = fraction;
                hitted = true;
                return fraction;
            }, origin.ToMVec2() * VConvert.DisplayToSim,
            (origin + direction * rayLength).ToMVec2() * VConvert.DisplayToSim);
            hit = info;
            return hitted;
        }
        public bool RayCast(Vector2 origin, Vector2 direction, float rayLength, out HitInfo hitInfo)
        {

            var info = new HitInfo();
            var hitted = false;
            float closetFract = 1;
            this.World.RayCast((fix, cp, cn, fraction) =>
            {
                if (fraction < closetFract )
                {
                    info.ContactPoint = cp * VConvert.SimToDisplay;
                    info.normal = cn;
                    info.fraction = fraction;
                    info.distance = fraction * rayLength;
                    info.fixture = fix;
                    hitted = true;

                    closetFract = fraction;
                }
                return fraction;
            }, origin.ToMVec2() * VConvert.DisplayToSim,
            (origin + direction * rayLength).ToMVec2() * VConvert.DisplayToSim);

            hitInfo = info;
            return hitted;
        }

        public bool RayCast(Func<HitInfo,float> callback,Vector2 origin, Vector2 direction, float rayLength, out HitInfo hitInfo)
        {

            var info = new HitInfo();
            var hitted = false;
            this.World.RayCast((fix, cp, cn, fraction) =>
            {
                info.ContactPoint = cp * VConvert.SimToDisplay;
                info.normal = cn;
                info.fraction = fraction;
                info.distance = fraction * rayLength;
                info.fixture = fix;

                info.fraction =  callback.Invoke(info);

                hitted = true;

                return info.fraction;
            }, origin.ToMVec2() * VConvert.DisplayToSim,
            (origin + direction * rayLength).ToMVec2() * VConvert.DisplayToSim);


            hitInfo = info;
            return hitted;
        }

        public struct HitInfo
        {
            public Vector2 ContactPoint, normal;
            public float distance , fraction;
            public Fixture fixture;
        }
    }



}