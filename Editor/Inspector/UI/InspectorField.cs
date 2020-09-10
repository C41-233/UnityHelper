using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Inspector
{
    internal abstract class InspectorField
    {

        protected readonly InspectorWindow window;
        
        protected InspectorField(InspectorWindow window)
        {
            this.window = window;
        }

        protected void DrawField(GUIContent labelName, object value, InspectorReflectType facadeType)
        {
            var type = value == null ? null : InspectorReflectType.Get(value.GetType());
            DrawField(labelName, value, facadeType, type);
        }

        protected void DrawField(GUIContent labelName, object value, InspectorReflectType facadeType, InspectorReflectType type)
        {
            if (InspectorPrettyPrint.TryPrint(value, out var str))
            {
                DirectDraw(labelName, str);
                return;
            }
            if (value is IEnumerable enumerable)
            {
                 if (type.RuntimeType.IsArray)
                 {
                     DrawList(labelName, enumerable, type, type.ElementType);
                     return;
                 }
                 if (type.RuntimeType.IsGenericType)
                 {
                     var genericTypeDefinition = type.RuntimeType.GetGenericTypeDefinition();
                     if (genericTypeDefinition == typeof(List<>))
                     {
                         DrawList(labelName, enumerable, type, InspectorReflectType.Get(type.RuntimeType.GetGenericArguments()[0]));
                         return;
                     }

                     if (genericTypeDefinition == typeof(Dictionary<,>))
                     {
                         DrawList(labelName, enumerable, type, null);
                         return;
                     }
                 }
                 
                 if (facadeType != null)
                 {
                     if (facadeType.RuntimeType.IsGenericType && facadeType.RuntimeType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                     {
                         DrawList(labelName, enumerable, type, null);
                         return;
                     }

                     if (facadeType.RuntimeType == typeof(IEnumerable))
                     {
                         DrawList(labelName, enumerable, type, null);
                         return;
                     }
                 }
            }

            DrawObject(labelName, value, type);
        }

        private void DirectDraw(GUIContent labelName, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelName, window.Style_DirectLabel);
            EditorGUILayout.LabelField(FilterLabelString(value));
            EditorGUILayout.EndHorizontal();
        }

        private static readonly StringBuilder StringBuilder = new StringBuilder();

        private static string FilterLabelString(string value)
        {
            var sb = StringBuilder;
            foreach (var ch in value)
            {
                switch (ch)
                {
                    case '\n': sb.Append(@"\n"); break;
                    case '\r': sb.Append(@"\r"); break;
                    default: sb.Append(ch); break;
                }   
            }

            var r = sb.ToString();
            sb.Clear();
            return r;
        }

        private bool active;
        private InspectorFieldContext ctx;
        
        private class InspectorFieldContext
        {
            public InspectorFieldBase BaseField;
            public InspectorFieldElement ForeachField;
            public List<InspectorFieldMember> Fields;
            public bool MethodActive;
            public List<InspectorFieldMethod> Methods;
            public InspectorReflectType Type;
        }
        
        private void DrawObject(GUIContent labelName, object value, InspectorReflectType type)
        {
            var expand = active;

            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            GUI.SetNextControlName("foldout" + RuntimeHelpers.GetHashCode(value));
            active = EditorGUILayout.Foldout(expand, labelName, true);
            EditorGUILayout.LabelField(type.TypeName, window.Style_ExpandLabel);
            EditorGUILayout.EndHorizontal();

            if (!expand)
            {
                return;
            }
            
            if (ctx == null || ctx.Type != type)
            {
                ctx = new InspectorFieldContext
                {
                    Type = type,
                };
                if (type.BaseType != null)
                {
                    ctx.BaseField = new InspectorFieldBase(window, type.BaseType);
                }
                if (type.IsEnumerable)
                {
                    ctx.ForeachField = new InspectorFieldElement(window, InspectorReflectType.Get(typeof(IEnumerable)), type);
                }
                
                ctx.Fields = new List<InspectorFieldMember>();

                foreach (var field in type.Fields)
                {
                    ctx.Fields.Add(new InspectorFieldMember(window, field));
                }

                ctx.Fields.TrimExcess();
                
                ctx.Methods = new List<InspectorFieldMethod>();
                foreach (var method in type.Methods)
                {
                    ctx.Methods.Add(new InspectorFieldMethod(window, method));
                }
                ctx.Methods.TrimExcess();
            }

            EditorGUILayout.BeginVertical(window.Style_Box, GUILayout.ExpandWidth(true));
            if (value is GameObject go && go != Selection.activeGameObject)
            {
                if (GUILayout.Button("select"))
                {
                    Selection.activeGameObject = go;
                    return;
                }
            }
            if (value is Component cp && cp.gameObject != Selection.activeGameObject)
            {
                if (GUILayout.Button("select"))
                {
                    Selection.activeGameObject = cp.gameObject;
                    return;
                }
            }
            ctx.BaseField?.Draw(value);
            ctx.ForeachField?.Draw($"[foreach]", value);
            foreach (var field in ctx.Fields)
            {
                field.Draw(value);
            }

            if (ctx.Methods.Count > 0)
            {
                var expendMethods = ctx.MethodActive;
                ctx.MethodActive = EditorGUILayout.Foldout(ctx.MethodActive, "[Methods]", true);
                if (expendMethods)
                {
                    EditorGUILayout.BeginVertical(window.Style_Box);
                    foreach (var method in ctx.Methods)
                    {
                        method.Draw(value);
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private List<InspectorFieldElement> list;
        
        private void DrawList(GUIContent labelName, IEnumerable enumerable, InspectorReflectType enumerableType, InspectorReflectType elementType)
        {
            var expand = active;
            
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
            active = EditorGUILayout.Foldout(active, labelName, true);
            if (InspectorReflect.TryGetCount(enumerable, out var count))
            {
                EditorGUILayout.LabelField($"{enumerableType.TypeName}{{{count}}}", window.Style_ExpandLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"{enumerableType.TypeName}", window.Style_ExpandLabel);
            }
            EditorGUILayout.EndHorizontal();
            
            if (!expand)
            {
                return;
            }
            EditorGUILayout.BeginVertical(window.Style_Box, GUILayout.ExpandWidth(true));

            if (list == null)
            {
                list = new List<InspectorFieldElement>();
            }
            
            var i = 0;
            foreach (var e in enumerable)
            {
                if (list.Count <= i || list[i].Type.RuntimeType != e?.GetType())
                {
                    var element = new InspectorFieldElement(window, elementType, InspectorReflectType.Get(e?.GetType()));
                    if (list.Count <= i)
                    {
                        list.Add(element);
                    }
                    else
                    {
                        list[i] = element;
                    }
                }

                list[i].Draw($"[{i}]", e);
                i++;
            }

            if (list.Count >= i)
            {
                list.RemoveRange(i, list.Count - i);
            }
            
            //表內无元素
            if (i == 0)
            {
                EditorGUILayout.LabelField($"<empty>");
            }

            EditorGUILayout.EndVertical();
        }

    }
}