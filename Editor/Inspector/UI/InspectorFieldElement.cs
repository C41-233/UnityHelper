using System;
using UnityEngine;

namespace Inspector
{
    internal class InspectorFieldElement : InspectorField
    {
        
        public InspectorReflectType Type { get; }
        
        public InspectorFieldElement(InspectorWindow window, InspectorReflectType facadeType, InspectorReflectType type) : base(window)
        {
            this.facadeType = facadeType;
            Type = type;
        }

        public void Draw(string label, object value)
        {
            var labelName = new GUIContent(label);
            DrawField(labelName, value, facadeType);
        }

        private readonly InspectorReflectType facadeType;

    }
}