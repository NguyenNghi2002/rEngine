
using SVector3 = System.Numerics.Vector3;
using SVector2 = System.Numerics.Vector2;
using MVector2 = Microsoft.Xna.Framework.Vector2;
using MVector3 = Microsoft.Xna.Framework.Vector3;

namespace Engine.Velcro.Unit
{

    public static class Vector2Ext
	{
        public static MVector2 FlipY(MVector2 vector, MVector2 point)
        {
            FlipY(ref vector, point);
            return vector;
        }
        public static void FlipY(ref MVector2 vector, MVector2 point)
        {
            var error = point.Y - vector.Y;
            vector.Y += error * 2f;
        }
        public static void FlipY(ref SVector2 vector, SVector2 point)
        {
            var error = point.Y - vector.Y;
            vector.Y += error * 2f;
        }

        public static MVector2 FlipX(MVector2 vector, MVector2 point)
        {
            FlipX(ref vector,point);
            return vector;
        }
        public static void FlipX(ref MVector2 vector, MVector2 point)
        {
            var error = point.X - vector.X;
            vector.X += error * 2f;
        }
        public static void FlipX(ref SVector2 vector,SVector2 point)
        {
            var error = point.X - vector.X;
            vector.X += error * 2f;
        }
        public static SVector2 Lerp(SVector2 from,SVector2 to,SVector2 lerp)
        {
            return new SVector2((to.X - from.X) * lerp.X + to.X, (to.Y - from.Y) * lerp.Y + to.Y);
        }


        #region Dimension Conversion
        public static MVector2 ToVec2(in this MVector3 value) => new MVector2(value.X, value.Y);
        public static MVector3 ToVec3(in this MVector2 value) => new MVector3(value.X, value.Y,0f);

        
        #endregion

        #region System -> XNA
        /// <summary>
        /// Convert from System Vector2 to XNA vector2
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MVector2 ToMVec2(in this SVector2 value) => new MVector2(value.X, value.Y);

        /// <summary>
        /// Convert from System Vector3 to XNA vector3
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MVector3 ToMVec3(in this SVector3 value) => new MVector3(value.X, value.Y, value.Z);

        /// <summary>
        /// Convert from System Vector2 to XNA vector3
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MVector3 ToMVec3(in this SVector2 value) => new MVector3(value.X, value.Y, 0f);

        /// <summary>
        /// Convert from System Vector3 to XNA vector2
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static MVector2 ToMVec2(in this SVector3 value) => new MVector2(value.X, value.Y);
        #endregion


        #region XNA ->System
        /// <summary>
        /// Convert from XNA Vector2 to System vector2
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SVector2 ToSVec2(in this MVector2 value)
            => new SVector2(value.X, value.Y);

        /// <summary>
        /// Convert from XNA Vector3 to System vector3
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SVector3 ToSVec3(in this MVector3 value) => new SVector3(value.X, value.Y, value.Z);

        /// <summary>
        /// Convert from XNA Vector2 to System vector3
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SVector3 ToSVec3(in this MVector2 value) => new SVector3(value.X, value.Y, 0f);

        /// <summary>
        /// Convert from XNA Vector3 to System vector2
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static SVector2 ToSVec2(in this MVector3 value) => new SVector2(value.X, value.Y); 
        #endregion

    }
}
