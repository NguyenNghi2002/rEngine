using System.Numerics;

namespace Engine
{
    public static class TransformatonExt
    {
        #region Move()
        public static Entity Move(this Entity entity, double dx, double dy, double dz)
            => Move(entity, (float)dx, (float)dy, (float)dz);
        public static Entity Move(this Entity entity, double dx, double dy)
            => Move(entity, (float)dx, (float)dy);
        public static Entity Move(this Entity entity, float dx, float dy)
            => Move(entity, dx, dy, 0);
        public static Entity Move(this Entity entity, float dx, float dy, float dz)
            => Move(entity, new Vector3(dx, dy, dz));
        public static Entity Move(this Entity entity, Vector2 delta)
            => Move(entity, new Vector3(delta, 0f));
        public static Entity Move(this Entity entity, Vector3 delta)
        {
            entity.Transform.Position += delta;
            return entity;
        } 
        #endregion

        #region MoveTo()
        public static Entity MoveTo(this Entity entity, Vector2 point)
            => MoveTo(entity, point.X, point.Y, 0f);
        public static Entity MoveTo(this Entity entity, Vector3 point)
            => MoveTo(entity, point.X, point.Y, point.Z);
        public static Entity MoveTo(this Entity entity, float x, float y)
            => MoveTo(entity, x, y, 0);
        public static Entity MoveTo(this Entity entity, float value)
            => MoveTo(entity, value, value, value);
        public static Entity MoveTo(this Entity entity, double x, double y, double z)
            => MoveTo(entity, (float)x, (float)y, (float)z);
        public static Entity MoveTo(this Entity entity, double x, double y)
            => MoveTo(entity, (float)x, (float)y, 0f);
        public static Entity MoveTo(this Entity entity, float x, float y, float z)
        {
            entity.Transform.LocalPosition = new Vector3(x, y, z);
            return entity;
        } 
        #endregion


        public static Entity Rotate(this Entity entity, float x, float y, float z)
        {
            if(x == 0 && y ==0 && z == 0) return entity;
            entity.Transform.EulerLocalRotation += new Vector3(x, y, z);
            return entity;
        }


        public static Entity Scale(this Entity entity, float x,float y,float z)
        {
            entity.Transform.LocalScale *= new Vector3(x,y,z);
            return entity;
        }
        public static Entity Scale(this Entity entity, float scale)
            => Scale(entity, scale, scale, scale);

        public static Entity ScaleTo(this Entity entity, float x, float y,float z)
        {
            entity.Transform.LocalScale = new Vector3(x,y,z);
            return entity;
        }

        public static Entity ScaleTo(this Entity entity, float scale)
            => ScaleTo(entity, scale, scale, scale);





    }
}
