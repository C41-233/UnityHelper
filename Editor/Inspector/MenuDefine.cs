using UnityEditor;

namespace Inspector
{
    internal static class MenuDefine
    {
        
        [MenuItem("Inspector/Object", false, 0)]
        public static void AddWindow()
        {
            var window = EditorWindow.GetWindow<InspectorWindow>(false, "ObjectInspector");
            window.ShowUtility();
        }

    }
}