using UnityEngine;
using UnityEngine.UI;

namespace TestApp.Utils
{
    public static class UIUtils
    {
        public static Color Set(this Color c, float? r = null, float? g = null, float? b = null, float? a = null) =>
            new Color(r ?? c.r, g ?? c.g, b ?? c.b, a ?? c.a);

        public static Color SetAlpha(this Color color, float alpha) =>
            new Color(color.r, color.g, color.b, alpha);

        public static Image SetColor(this Image i, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            i.color = i.color.Set(r, g, b, a);
            return i;
        }

        public static SpriteRenderer SetColor(this SpriteRenderer i, float? r = null, float? g = null, float? b = null, float? a = null)
        {
            i.color = i.color.Set(r, g, b, a);
            return i;
        }
    }
}
