using Genbox.VelcroPhysics.Dynamics;
using Raylib_cs;

namespace Engine.Velcro
{
    internal static class VDebugDrawInfo
    {
        internal static float centerReadius { get; set; } = 3f;
        internal static float lineWidth = 1f;
        internal static float collidedLineWidth { get; set; } = 2f;

        internal static Color dynamicColor = Color.LIGHTGRAY;
        internal static Color kinematicColor = Color.DARKBLUE;
        internal static Color staticColor = Color.RED;

        internal static float contactPoint0Radius = 3;
        internal static float contactPoint1Radius = 3;

        internal static float contactNormal0Length = 20;
        internal static float contactNormal1Length = 20;

        internal static float contactNormal0LineWidth = 1;
        internal static float contactNormal1LineWidth = 1;

        internal static Color contactPoint0Color = Color.GOLD;
        internal static Color contactNormal0Color = Color.GOLD;

        internal static Color contactPoint1Color = Color.BEIGE;
        internal static Color contactNormal1Color = Color.BEIGE;
        internal static Color centerColor = Color.SKYBLUE;

        internal static float GetWidth (this Body body)
        {
            return body.ContactList != null ? collidedLineWidth : lineWidth;
        }
        internal static Color GetColor(this Body body)
        {
          
            return body.BodyType switch
            {
                BodyType.Static => staticColor,
                BodyType.Kinematic => kinematicColor,
                BodyType.Dynamic => dynamicColor,
                _ => Color.WHITE,
            };
        }

    }



}