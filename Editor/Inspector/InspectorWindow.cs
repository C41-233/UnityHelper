using System;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Inspector
{

    internal partial class InspectorWindow : EditorWindow
    {
        
        private Texture2D tooltipBackground;

        public GUIStyle Style_MonoBehaviourName;
        public GUIStyle Style_DirectLabel;
        public GUIStyle Style_ExpandLabel;
        public GUIStyle Style_Box;
        
        private void OnEnable()
        {
            tooltipBackground = new Texture2D(Screen.width, Screen.height);
            var pixels = Enumerable.Repeat(Color.gray, Screen.width * Screen.height).ToArray();
            tooltipBackground.SetPixels(pixels);
            tooltipBackground.Apply();

            EditorApplication.playModeStateChanged += OnPlayModeChange;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChange;
            Clear();
        }

        private void Clear()
        {
            current = null;
            contexts.Clear();
        }
        
        private void OnPlayModeChange(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode || change == PlayModeStateChange.ExitingPlayMode)
            {
                Clear();
            }
            Repaint();
        }
        
        private readonly InspectorCache<InspectorWindowContext> contexts = new InspectorCache<InspectorWindowContext>();
        
        private InspectorWindowContext current;
        
        private void OnSelectionChange()
        {
            Repaint();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private void ResetObject()
        {
            var go = current?.Go;
            if (current != null && go == null)
            {
                current = null;
            }
            
            var active = Selection.activeGameObject;

            if (active == null)
            {
                if (current == null)
                {
                    return;
                }

                current = null;
                return;
            }

            if (current == null || !ReferenceEquals(go, active))
            {
                current = contexts.Get(active);
                if (current == null)
                {
                    current = new InspectorWindowContext(this, active);
                    contexts.Put(active, current);
                }
            }
            
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Only work in play mode.");
                return;
            }

            ResetObject();
            
            if (current == null)
            {
                EditorGUILayout.LabelField("Wait for selecting a GameObject.");
                return;
            }

            Style_MonoBehaviourName = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
            Style_DirectLabel = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(15, 0, 0, 0),
            };
            Style_ExpandLabel = new GUIStyle(EditorStyles.label)
            {
                padding = new RectOffset(80, 0, 0, 0)
            };
            Style_Box = new GUIStyle(GUI.skin.box)
            {
                margin = new RectOffset(10, 0, 0, 0)
            };

            current.Draw();

            if (GUI.GetNameOfFocusedControl().StartsWith("foldout"))
            {                
                GUI.FocusControl(null);
            }
            
            if (Event.current.type == EventType.Repaint && !string.IsNullOrEmpty(GUI.tooltip))
            {
                DrawTooltip();
            }
        }

        private void DrawTooltip()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                wordWrap = true,
            };
            
            var mp = Event.current.mousePosition;
            var str = CalculateSize(GUI.tooltip, out var width, out var height);
            
            var x = mp.x;
            if (x + width + 20 > position.width)
            {
                x = position.width - width - 20;
                if (x < 0)
                {
                    x = 0;
                }
            }

            var y = mp.y - height;
            if (y < 0)
            {
                y = 0;
            }

            var rect = new Rect(x, y, width, height);
            
            GUI.DrawTexture(rect, tooltipBackground);
            GUI.color = Color.white;
            GUI.Label(rect, str, style);
        }
        
        private static readonly StringBuilder StringBuilder = new StringBuilder();

        private string CalculateSize(string value, out float width, out float height)
        {
            const float W = 7.2f;
            const float H = 20f;

            var c_per_line = Math.Max((int) (position.width / W), 5);
            
            var sb = StringBuilder;
            var tokens = value.Split('\n');

            var col = 0;
            var row = 0;
            
            foreach (var token in tokens)
            {
                col = Math.Max(col, token.Length);
                for (int i=0, j=0; i<token.Length; i++, j++)
                {
                    if (j >= c_per_line)
                    {
                        j = 0;
                        sb.Append('\n');
                        row++;
                    }

                    sb.Append(token[i]);
                }

                if (row > 0)
                {
                    sb.Append('\n');
                }
                row++;
            }
            width = Math.Min(col * W, position.width);
            height = row * H;
            
            var r = sb.ToString();
            sb.Clear();
            return r;
        }

    }

}