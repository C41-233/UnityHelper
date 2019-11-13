
using System;
using UnityEngine;

namespace Inspector
{
    internal static class InspectorPrettyPrint
    {

        private const string FloatStyle = "0.#######";

        public static string Print(object value)
        {
            return TryPrint(value, out var str) ? str : null;
        }
        
        public static bool TryPrint(object value, out string str)
        {
            if (value == null)
            {
                str = "<null>";
                return true;
            }

            if (InspectorReflect.IsUnityNull(value))
            {
                str = "<unassigned>";
                return true;
            }

            if (value is float f)
            {
                str = f.ToString(FloatStyle);
                return true;
            }

            {
                if (value is double d)
                {
                    str = d.ToString(FloatStyle);
                    return true;
                }
            }
            {
                if (value is decimal d)
                {
                    str = d.ToString(FloatStyle);
                    return true;
                }
            }

            var type = value.GetType();
            if (value is string || type.IsPrimitive)
            {
                str = value.ToString();
                return true;
            }
 
            switch (value)
            {
                case Vector2 v2:
                    str = $"<{v2.x.ToString(FloatStyle)}, {v2.y.ToString(FloatStyle)}>";
                    return true;
                case Vector3 v3:
                    str = $"<{v3.x.ToString(FloatStyle)}, {v3.y.ToString(FloatStyle)}, {v3.z.ToString(FloatStyle)}>";
                    return true;
                case Quaternion q:
                    str = $"<{q.x.ToString(FloatStyle)}, {q.y.ToString(FloatStyle)}, {q.z.ToString(FloatStyle)}, {q.w.ToString(FloatStyle)}>";
                    return true;
                case Delegate dl:
                    str = dl.Method.ToString();
                    return true;
            }

            if (type.IsEnum)
            {
                var enumName = type.GetEnumName(value);
                str = enumName;
                return true;
            }

            str = null;
            return false;
        }
        
    }
}