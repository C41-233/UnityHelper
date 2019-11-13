using System;
using UnityEditor;
using UnityEngine;

namespace Inspector
{
    internal class InspectorFieldBase : InspectorField
    {

        private readonly InspectorReflectType type;
        
        public InspectorFieldBase(InspectorWindow window, InspectorReflectType type) : base(window)
        {
            this.type = type;
        }

        public void Draw(object value)
        {
            var label = new GUIContent("base");
            if (type.RuntimeType == typeof(object))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, window.Style_DirectLabel);
                EditorGUILayout.LabelField($"object");
                EditorGUILayout.EndHorizontal();
                return;
            }
            DrawField(label, value, null, type);
        }
    }
}