using Engine;
using ImGuiNET;
using System.Numerics;
namespace Undo
{
    public struct MoveCommand : CommandSystem.ICommand
    {
        public Vector2 PrevPosition;
        public Vector2 Direction;
        public float MoveDistance;

        public Transformation Context;
        public MoveCommand(Transformation context, Vector2 direction, float movedistance)
        {
            this.Context = context;
            this.PrevPosition = context.Position2;
            this.Direction = direction;
            this.MoveDistance = movedistance;
        }


        void CommandSystem.ICommand.Execute()
        {
            Context.Position2 += Direction * MoveDistance;
        }

        void CommandSystem.ICommand.Redo()
        {
            Context.Position2 += Direction * MoveDistance;
        }

        void CommandSystem.ICommand.Undo()
        {
            Context.Position2 -= Direction * MoveDistance;
        }
    }

}
