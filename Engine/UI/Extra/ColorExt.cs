using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Raylib_cs.UI.Extra
{
    internal static class ColorExt
    {
        public static Color Create(Color color, int alpha) => new Color(color.r, color.g, color.b, alpha);
        internal static bool HasValue(this Color self)
            => self.r != 0
            && self.g != 0
            && self.b != 0
            && self.a != 0;

    }
}
