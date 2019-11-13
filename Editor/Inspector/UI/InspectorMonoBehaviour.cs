using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Inspector
{
    internal class InspectorMonoBehaviour
    {

        private readonly InspectorWindow window;
        
        private readonly InspectorReflectType type;

        private readonly WeakReference<MonoBehaviour> host;
        
        public InspectorMonoBehaviour(InspectorWindow window, MonoBehaviour monoBehaviour)
        {
            this.window = window;
            host = new WeakReference<MonoBehaviour>(monoBehaviour);
            type = InspectorReflectType.Get(monoBehaviour.GetType());
        }

        private bool active;

        public void Draw()
        {
            if (!active)
            {
                active = EditorGUILayout.Foldout(false, type.TypeName, true, window.Style_MonoBehaviourName);
            }
            else
            {
                active = EditorGUILayout.Foldout(true, type.TypeName, true, window.Style_MonoBehaviourName);
                DrawFields();
            }
        }

        private List<InspectorFieldMember> fields;
        private InspectorFieldBase baseField;
        private bool methodActive;
        private List<InspectorFieldMethod> methods;
        
        private void DrawFields()
        {
            if (fields == null)
            {
                fields = new List<InspectorFieldMember>(type.Fields.Count);
                foreach (var field in type.Fields)
                {
                    fields.Add(new InspectorFieldMember(window, field));
                }
            }

            if (baseField == null && type.BaseType != null)
            {
                baseField = new InspectorFieldBase(window, type.BaseType);
            }

            if (methods == null)
            {
                methods = new List<InspectorFieldMethod>(type.Methods.Count);
                foreach (var method in type.Methods)
                {
                    methods.Add(new InspectorFieldMethod(window, method));
                }
            }
            
            EditorGUILayout.BeginVertical(window.Style_Box);
            if (host.TryGetTarget(out var target) && target != null)
            {
                baseField?.Draw(target);

                foreach (var field in fields)
                {
                    field.Draw(target);
                }

                var expandMethod = methodActive;
                methodActive = EditorGUILayout.Foldout(methodActive, "[Methods]", true);
                if (expandMethod)
                {
                    EditorGUILayout.BeginVertical(window.Style_Box);
                    foreach (var method in methods)
                    {
                        method.Draw(target);
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndVertical();
        }
        
    }
}