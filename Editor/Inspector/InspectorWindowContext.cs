using System;
using UnityEditor;
using UnityEngine;

namespace Inspector
{

    internal partial class InspectorWindow
    {
        
        internal class InspectorWindowContext
        {

            private readonly WeakReference<GameObject> reference;

            public GameObject Go
            {
                get
                {
                    if (reference.TryGetTarget(out var target))
                    {
                        return target;
                    }

                    return null;
                }
            }
            
            private readonly InspectorCache<InspectorMonoBehaviour> monobehaviours = new InspectorCache<InspectorMonoBehaviour>();

            private readonly InspectorWindow window;

            public InspectorWindowContext(InspectorWindow window, GameObject go)
            {
                this.window = window;
                reference = new WeakReference<GameObject>(go); 
            }

            private Vector2 scroll;
            
            public void Draw()
            {
                var go = Go;
                
                EditorGUILayout.LabelField("Name", go.name);
                
                scroll = EditorGUILayout.BeginScrollView(scroll);

                EditorGUILayout.LabelField("WorldPosition", InspectorPrettyPrint.Print(go.transform.position));
                EditorGUILayout.LabelField("WorldEulerAngles", InspectorPrettyPrint.Print(go.transform.eulerAngles));
                EditorGUILayout.LabelField("LossyScale", InspectorPrettyPrint.Print(go.transform.lossyScale));
                
                EditorGUILayout.LabelField("LocalPosition", InspectorPrettyPrint.Print(go.transform.localPosition));
                EditorGUILayout.LabelField("LocalEulerAngles", InspectorPrettyPrint.Print(go.transform.localEulerAngles));
                EditorGUILayout.LabelField("LocalScale", InspectorPrettyPrint.Print(go.transform.localScale));
                
                foreach (var component in go.GetComponents<MonoBehaviour>())
                {
                    var node = monobehaviours.Get(component);
                    if (node == null)
                    {
                        node = new InspectorMonoBehaviour(window, component);
                        monobehaviours.Put(component, node);
                    }
                    
                    EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
                    node.Draw();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndScrollView();
            }
        }
        
    }
    
}