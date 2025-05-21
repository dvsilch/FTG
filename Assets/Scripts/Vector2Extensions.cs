using UnityEngine;

namespace Dvsilch;

public static class Vector2Extensions
{
    public static ButtonMapping Vector2ButtonMapping(this Vector2 vector2)
    {
        if (vector2.magnitude > 0.5f)
        {
            var angle = (Mathf.Atan2(vector2.y, vector2.x) * Mathf.Rad2Deg + 360) % 360;
            if (angle >= 337.5f || angle < 22.5f)
                return ButtonMapping.Right;
            else if (angle >= 22.5f && angle < 67.5f)
                return ButtonMapping.RightUp;
            else if (angle >= 67.5f && angle < 112.5f)
                return ButtonMapping.Up;
            else if (angle >= 112.5f && angle < 157.5f)
                return ButtonMapping.LeftUp;
            else if (angle >= 157.5f && angle < 202.5f)
                return ButtonMapping.Left;
            else if (angle >= 202.5f && angle < 247.5f)
                return ButtonMapping.LeftDown;
            else if (angle >= 247.5f && angle < 292.5f)
                return ButtonMapping.Down;
            else if (angle >= 292.5f && angle < 337.5f)
                return ButtonMapping.RightDown;
        }

        return ButtonMapping.None;
    }
}