using SVector3 = System.Numerics.Vector3;
using SVector2 = System.Numerics.Vector2;

namespace Engine
{
    public static class SystemVector2Ext
    {
        public static SVector2 ToVec2(in this SVector3 value) => new SVector2(value.X, value.Y);
        public static SVector2 ToInt2(in this SVector2 value) => new SVector2((int)value.X, (int)value.Y);

        public static SVector3 ToVec3(in this SVector2 value) => new SVector3(value.X, value.Y, 0f);
        public static SVector3 ToInt3(in this SVector3 value) => new SVector3((int)value.X, (int)value.Y,(int)value.Z);
    }
}
