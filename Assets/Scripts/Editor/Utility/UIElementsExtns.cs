using UnityEngine.UIElements;

namespace Editor.Utility
{
    public static class UIElementsExtns
    {
        public static void AddClasses(VisualElement element, params string[] classes)
        {
            foreach (var @class in classes)
            {
                element.AddToClassList(@class);
            }
        }

        public static T AddClasses<T>(this T element, params string[] classes) where T : VisualElement
        {
            foreach (var @class in classes)
            {
                element.AddToClassList(@class);
            }

            return element;
        }
    }
}