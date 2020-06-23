using System;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Editor.Utility
{
    [CustomPropertyDrawer(typeof(StringValidationAttribute))]
    public class StringLengthDrawer : PropertyDrawer
    {
        private StringValidationAttribute lenAttr;

        public override void OnGUI(Rect position, SerializedProperty property,
            GUIContent                  label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property);
            }

            lenAttr = (StringValidationAttribute) attribute;
            if (lenAttr == null) return;

            string value = string.Empty;

            EditorGUI.BeginChangeCheck();
            if (lenAttr.TextArea)
                value = EditorGUI.TextArea(position, property.displayName, property.stringValue);
            else
                value = EditorGUI.TextField(position, property.displayName, property.stringValue);

            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = ApplyOperations(value);
            }
        }

        private string ApplyOperations(string value)
        {
            if (lenAttr.Operations.HasFlag(StringOperations.MaxLength))
            {
                value = value.Substring(0, Math.Min(lenAttr.MaxLength, value.Length));
            }

            if (lenAttr.Operations.HasFlag(StringOperations.Upper))
            {
                value = value.ToUpper();
            }

            if (lenAttr.Operations.HasFlag(StringOperations.Lower))
            {
                value = value.ToLower();
            }

            if (lenAttr.Operations.HasFlag(StringOperations.Trim))
            {
                value = value.Trim(lenAttr.Trim);
            }

            return value;
        }
    }
}