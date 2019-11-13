using UnityEditor;
using UnityEngine;

namespace Inspector
{
    
    internal class InspectorFieldMethod
    {

        private readonly InspectorWindow window;

        private readonly InspectorReflectMethod method;
        
        public InspectorFieldMethod(InspectorWindow window, InspectorReflectMethod method)
        {
            this.window = window;
            this.method = method;
        }

        private string text;
        
        public void Draw(object value)
        {
            EditorGUILayout.BeginHorizontal();
            var content = new GUIContent(method.Name, method.Descriptor);
            EditorGUILayout.LabelField(content, window.Style_DirectLabel);
            text = GUILayout.TextField(text, GUILayout.Width(200));
            if (GUILayout.Button("Call", GUILayout.Width(60)))
            {
                MethodCall(value);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void MethodCall(object value)
        {
            var result = method.Call(value, null);
            if (!InspectorPrettyPrint.TryPrint(result, out var str))
            {
                str = result.ToString();
            }
            Debug.LogWarning($"[Editor Inspector] {method.Name} {str}");
        }
        
    }
    
}