using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Inspector
{
    internal static class InspectorReflect
    {

        public static bool IsCompilerGenerated(MemberInfo member)
        {
            return member.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
        }
        
        public static string NormalizeName(string name)
        {
            var idot = name.LastIndexOf('.');
            if(idot < 1)
            {
                return name;
            }
            idot = name.LastIndexOf('.', idot - 1);
            if (idot < 0)
            {
                return name;
            }
            
            name = name.Substring(idot + 1);
            return name;
        }

        public static bool IsUnityNull(object value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is UnityEngine.Object go)
            {
                return go == null;
            }

            return false;
        }

    }
}