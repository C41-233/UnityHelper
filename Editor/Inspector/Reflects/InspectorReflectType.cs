using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Inspector
{
    internal class InspectorReflectType
    {
        
        #region static

        static InspectorReflectType()
        {
            Array.Sort(ExcludeFields);
        }


        private static readonly Dictionary<Type, InspectorReflectType> types = new Dictionary<Type, InspectorReflectType>();
        
        public static InspectorReflectType Get(Type type)
        {
            if (type == null)
            {
                return null;
            }
            if (!types.TryGetValue(type, out var rt))
            {
                rt = new InspectorReflectType(type);
                types.Add(type, rt);
            }
            return rt;
        }
        
        private static readonly string[] ExcludeFields =
        {
            "animation", "audio", "camera", "collider", "collider2D", "constantForce", "guiElement", "guiText", "guiTexture"
            , "hingeJoint", "light", "networkView", "particleSystem", "renderer", "rigidbody", "rigidbody2D"
        };

        #endregion

        public string TypeName => typename ?? (typename = GetTypeName(RuntimeType, false));

        private string typename;
        
        public string TypeFullName => typefullname ?? (typefullname = GetTypeName(RuntimeType, true));

        private string typefullname;

        public bool IsKeyValuePair { get; }

        public bool IsEnumerable { get; }
        
        private InspectorReflectType(Type type)
        {
            RuntimeType = type;
            IsKeyValuePair = RuntimeType.IsGenericType && RuntimeType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>);
            IsEnumerable = typeof(IEnumerable).IsAssignableFrom(type);
        }

        public Type RuntimeType { get; }

        #region BaseType
        public InspectorReflectType BaseType
        {
            get
            {
                if(baseType != null)
                {
                    return baseType;
                }
                if (RuntimeType.BaseType == null || IsKeyValuePair || RuntimeType.IsValueType)
                {
                    return null;
                }

                baseType = Get(RuntimeType.BaseType);
                return baseType;
            }
        }

        private InspectorReflectType baseType;
        #endregion

        #region Fields

        public IReadOnlyList<InspectorReflectField> Fields
        {
            get
            {
                if (fields != null)
                {
                    return fields;
                }

                fields = new List<InspectorReflectField>();
                if (IsKeyValuePair)
                {
                    fields.Add(new InspectorReflectField(RuntimeType.GetProperty("Key")));
                    fields.Add(new InspectorReflectField(RuntimeType.GetProperty("Value")));
                }
                else
                {
                    foreach (var field in RuntimeType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (InspectorReflect.IsCompilerGenerated(field))
                        {
                            continue;
                        }

                        fields.Add(new InspectorReflectField(field));
                    }
                
                    foreach (var property in RuntimeType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        if (InspectorReflect.IsCompilerGenerated(property))
                        {
                            continue;
                        }

                        if (!property.CanRead)
                        {
                            continue;
                        }

                        if (property.GetMethod.GetParameters().Length != 0)
                        {
                            continue;
                        }
                
                        if((property.DeclaringType == typeof(MonoBehaviour) 
                            || property.DeclaringType == typeof(Behaviour) 
                            || property.DeclaringType == typeof(Component)
                            || property.DeclaringType == typeof(GameObject)
                           ) && Array.BinarySearch(ExcludeFields, property.Name) >= 0
                        )
                        {
                            continue;
                        }
                
                        var node = new InspectorReflectField(property);
                        fields.Add(node);
                    }
                }
                
                fields.Sort((x, y) => string.CompareOrdinal(x.Name, y.Name));
                fields.TrimExcess();
                return fields;
            }
        }

        private List<InspectorReflectField> fields;

        #endregion

        #region Methods

        public IReadOnlyList<InspectorReflectMethod> Methods
        {
            get
            {
                if (methods != null)
                {
                    return methods;
                }
                methods = new List<InspectorReflectMethod>();
                CreateMethods();
                methods.Sort((m1, m2) => string.CompareOrdinal(m1.Name, m2.Name));
                methods.TrimExcess();
                return methods;
            }
        }

        private List<InspectorReflectMethod> methods;

        private void CreateMethods()
        {
            if (IsKeyValuePair)
            {
                return;
            }

            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            if (RuntimeType.IsValueType)
            {
                flags |= BindingFlags.DeclaredOnly;
            }
            foreach (var method in RuntimeType.GetMethods(flags))
            {
                if (method.IsSpecialName || InspectorReflect.IsCompilerGenerated(method))
                {
                    continue;
                }
                var m = new InspectorReflectMethod(method);
                methods.Add(m);
            }
        }
        #endregion

        public InspectorReflectType ElementType => Get(RuntimeType.GetElementType());
        
        private static string GetTypeName(Type type, bool fullname)
        {
            if (type == typeof(object))
            {
                return "object";
            }
            if (type == typeof(string))
            {
                return "string";
            }
            if (type == typeof(byte))
            {
                return "byte";
            }
            if (type == typeof(sbyte))
            {
                return "sbyte";
            }
            if (type == typeof(char))
            {
                return "char";
            }
            if (type == typeof(short))
            {
                return "short";
            }
            if (type == typeof(ushort))
            {
                return "ushort";
            }
            if (type == typeof(int))
            {
                return "int";
            }
            if (type == typeof(uint))
            {
                return "uint";
            }
            if (type == typeof(long))
            {
                return "long";
            }
            if (type == typeof(ulong))
            {
                return "ulong";
            }
            if (type == typeof(float))
            {
                return "float";
            }
            if (type == typeof(double))
            {
                return "double";
            }
            if (type == typeof(decimal))
            {
                return "decimal";
            }
            if (type == typeof(bool))
            {
                return "bool";
            }

            if (type == typeof(void))
            {
                return "void";
            }

            var parse = FullParse(type, fullname);
            if (fullname)
            {
                return type.Namespace == null ? parse : type.Namespace + "." + parse;
            }

            return parse;
        }

        private static string FullParse(Type type, bool fullname)
        {
            if (type.IsGenericType)
            {
                var sb = new StringBuilder();
                var genericName = type.Name;
                var iQuota = genericName.IndexOf('`');
    
                if (iQuota > 0)
                {
                    sb.Append(genericName.Substring(0, iQuota));
                }
                else
                {
                    sb.Append(genericName);
                }

                sb.Append('<');
                sb.Append(string.Join(", ", type.GetGenericArguments().Select(t =>
                {
                    var tt = Get(t);
                    return fullname ? tt.TypeFullName : tt.TypeName;
                })));
                sb.Append('>');
                return sb.ToString();
            }

            return type.Name;
        }
    }
}