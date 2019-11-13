using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Inspector
{
    internal class InspectorFieldMember : InspectorField
    {
        private readonly InspectorReflectField field;
        
        public InspectorFieldMember(InspectorWindow window, InspectorReflectField field) : base(window)
        {
            this.field = field;
        }
        
        public void Draw(object host)
        {
            var labelName = new GUIContent(field.Name, field.Descriptor);
            object cacheValue;
            try
            {
                cacheValue = field.GetValue(host);
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException targetException)
                {
                    e = targetException.InnerException;
                }
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(labelName, window.Style_DirectLabel);
                var body = new GUIContent($"<{e.GetType().Name}>", e.ToString());
                EditorGUILayout.LabelField(body);
                EditorGUILayout.EndHorizontal();
                return;
            }
            
            DrawField(labelName, cacheValue, field.Type);
        }

    }
}