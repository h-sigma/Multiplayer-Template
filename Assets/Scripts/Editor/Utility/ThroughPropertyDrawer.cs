using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Utility;

namespace Editor.Utility
{
    [CustomPropertyDrawer(typeof(ThroughPropertyAttribute))]
    public class ThroughPropertyDrawer : PropertyDrawer
    {
        private ThroughPropertyAttribute throughAttr;
        private SerializedProperty currentProperty;

        private PropertyInfo _propertyInfo;
        private MethodInfo _getter;
        private MethodInfo _setter;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            throughAttr = (ThroughPropertyAttribute) attribute;
            currentProperty = property;

            if (_getter == null)
            {
                GetGetter();
                if (_getter == null)
                {
                    EditorGUILayout.HelpBox(new GUIContent("Getter missing on property for ThroughPropertyAttribute."));
                    return;
                }
            }
            if (_setter == null) GetSetter();
            
            var value = _getter.Invoke(property.serializedObject.targetObject, new object[0]);
            
            EditorGUI.BeginChangeCheck();
            
            if (_setter == null) GUI.enabled = false;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean:
                    value = EditorGUI.Toggle(position, property.displayName, (bool) value);
                    break;
                case SerializedPropertyType.String:
                    value = EditorGUI.TextField(position, property.displayName, (string) value);
                    break;
                default:
                    UnityEngine.Debug.Log("owo");
                    return;
            }

            if (_setter == null) GUI.enabled = true;

            if (EditorGUI.EndChangeCheck() && _setter != null)
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Set value of " + property.displayName);
                _setter.Invoke(property.serializedObject.targetObject, new object[] {value});
            }
        }

        private void GetGetter()
        {
            if(_propertyInfo == null)
            {
                var srlPropertyPath    = currentProperty.propertyPath;
                var csharpPropertyPath = srlPropertyPath.Replace(currentProperty.name, throughAttr.propertySourceField);

                _propertyInfo = GetPropertyInfo(currentProperty, csharpPropertyPath);
            }   
            _getter = _propertyInfo.GetGetMethod(true);
        }

        private void GetSetter()
        {
            if(_propertyInfo == null)
            {
                var srlPropertyPath    = currentProperty.propertyPath;
                var csharpPropertyPath = srlPropertyPath.Replace(currentProperty.name, throughAttr.propertySourceField);

                _propertyInfo = GetPropertyInfo(currentProperty, csharpPropertyPath);
            }   
            _setter = _propertyInfo.GetSetMethod(true);
        }

        public static PropertyInfo GetPropertyInfo(SerializedProperty property, string csharpPropPath)
        {
            object obj = property.serializedObject.targetObject;

            var          flag  = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var          paths = csharpPropPath.Split('.');
            PropertyInfo prop  = null;

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (obj == null)
                    throw new System.NullReferenceException("Can't set a value on a null instance");

                var type = obj.GetType();
                if (path == "Array")
                {
                    path = paths[++i];
                    var iter = (obj as System.Collections.IEnumerable);
                    if (iter == null)
                        //Property path thinks this property was an enumerable, but isn't. property path can't be parsed
                        throw new System.ArgumentException("SerializedProperty.PropertyPath [" + csharpPropPath +
                                                           "] thinks that [" + paths[i - 2] + "] is Enumerable.");

                    var sind  = path.Split('[', ']');
                    int index = -1;

                    if (sind == null || sind.Length < 2)
                        // the array string index is malformed. the property path can't be parsed
                        throw new System.FormatException("PropertyPath [" + csharpPropPath + "] is malformed");

                    if (!Int32.TryParse(sind[1], out index))
                        //the array string index in the property path couldn't be parsed,
                        throw new System.FormatException("PropertyPath [" + csharpPropPath + "] is malformed");

                    obj = ElementAtOrDefault(iter, index);
                    continue;
                }

                prop = type.GetProperty(path, flag);
                if (prop == null)
                    //field wasn't found
                    throw new System.MissingFieldException("The field [" + path + "] in [" + csharpPropPath +
                                                           "] could not be found");

                if (i < paths.Length - 1)
                    obj = prop.GetValue(obj);
            }

            if (prop == null) throw new System.Exception();

            return prop;
        }
        
        public static PropertyInfo GetPropertyInfo(SerializedProperty property, string csharpPropPath,
            Type                                                      chsarpPropType)
        {
            var prop = GetPropertyInfo(property, csharpPropPath);

            if (chsarpPropType == null || property.serializedObject.targetObject.GetType() == null || !prop.PropertyType.IsAssignableFrom(chsarpPropType))
            {
                throw new System.InvalidCastException("Cannot cast ["   + chsarpPropType + "] into Field type [" +
                                                      prop.PropertyType + "]");
            }

            return prop;
        }

        public static System.Object ElementAtOrDefault(System.Collections.IEnumerable collection, int index)
        {
            var enumerator = collection.GetEnumerator();
            int j          = 0;
            for (; enumerator.MoveNext(); j++)
            {
                if (j == index) break;
            }

            System.Object element = (j == index)
                ? enumerator.Current
                : default(System.Object);

            var disposable = enumerator as System.IDisposable;
            if (disposable != null) disposable.Dispose();

            return element;
        }
    }
}