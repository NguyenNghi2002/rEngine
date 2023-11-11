
using Engine;
using Engine.Velcro;
using Raylib_cs;
using System.Numerics;

namespace Trex_runner
{
    public class JumpController : Component, IUpdatable
    {
        private Jumper jumperManager;
        public int UpdateOrder { get; set; }

        public override void OnAddedToEntity()
        {
            
            Entity.TryGetComponent(out jumperManager);
        }
        public void Update()
        {
            if (Input.IsKeyDown(KeyboardKey.KEY_LEFT))
                if (Entity.TryGetComponent<VRigidBody2D>(out var rb))
                    rb.ApplyForce(-Vector2.UnitX * 20f * Time.DeltaTime);

            if (Input.IsKeyDown(KeyboardKey.KEY_RIGHT))
                if (Entity.TryGetComponent<VRigidBody2D>(out var rb))
                    rb.ApplyForce( Vector2.UnitX * 20f * Time.DeltaTime);

            if (Input.IsKeyDown(KeyboardKey.KEY_UP ))
                jumperManager?.Jump();
            if (Input.IsKeyReleased(KeyboardKey.KEY_UP))
                jumperManager?.CutJump();
            if (Input.IsKeyDown(KeyboardKey.KEY_DOWN))
                jumperManager?.Duck();

            
        }
    }
}