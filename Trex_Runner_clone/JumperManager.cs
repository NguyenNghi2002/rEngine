
using Engine;
using Engine.Renderering;
using Engine.Timer;
using Engine.Velcro;
using Engine.Velcro.Unit;
using Genbox.VelcroPhysics.Collision.Filtering;
using Genbox.VelcroPhysics.Collision.Narrowphase;
using Genbox.VelcroPhysics.Collision.Shapes;
using Genbox.VelcroPhysics.Dynamics;
using Genbox.VelcroPhysics.Factories;
using Raylib_cs;
using System.Numerics;
using MVec2 = Microsoft.Xna.Framework.Vector2;

namespace Trex_runner
{

    public class Jumper : Component
    {
        public float jumpStrength = 100;
        public float duckStrength = 1;

        public JumpController controller;
        public VRigidBody2D rb;
        public bool isGrounded;
        public bool isDucking;
        

        public int UpdateOrder { get; set; }

        private VCollisionCircle groundSensor;

        private Vector2 offset;
        public override void OnAddedToEntity()
        {
            
            Entity.TryGetComponent(out controller);
            if(Entity.TryGetComponent(out rb))
            {
                rb.Body.GravityScale = 2f;
                rb.Body.FixedRotation = true;
            }

            Entity.TryGetComponent(out groundSensor);

            groundSensor.IsSensor = true;
            groundSensor.CollidesWith = Category.Cat2;
            groundSensor. OnCollision += (a, b, c) =>
            {
                isGrounded = true;
            };
            groundSensor.OnSeparation += (a, b, c) =>
            {
                isGrounded = false;
            };

        }


        public void Duck()
        {
            if (!isGrounded || !isDucking)
            {
                var force = new Vector2(rb.Body.LinearVelocity.X, duckStrength * VConvert.DisplayToSim * Time.DeltaTime);

                rb.Body.ApplyLinearImpulse(force.ToMVec2());
            }
        }
        private ITimer jumpTimer;
        public void Jump()
        {
            
            if (isGrounded || jumpTimer != null)
            {
                if(jumpTimer == null)
                    jumpTimer = Core.Schedule(0.2f, false, this, (timer) =>
                {
                    (timer.Context as Jumper).jumpTimer = null;
                });
                var force = new MVec2(rb.Body.LinearVelocity.X, -jumpStrength *VConvert.DisplayToSim);
                rb.Body.LinearVelocity = force;
            }
        }

        internal void CutJump()
        {
            jumpTimer?.Stop();
            jumpTimer = null;
        }

 
    }
}