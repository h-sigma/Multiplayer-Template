using UnityEditor;
using UnityEngine;
using Utility;
using MessageType = Utility.MessageType;

namespace Editor.Utility
    {
        [CustomPropertyDrawer(typeof(HelpBoxAttribute))]
        public class HelpBoxDecoratorDrawer : DecoratorDrawer
        {
            public HelpBoxAttribute helpBox => attribute as HelpBoxAttribute;

            public GUIContent lastContent;
            public Rect       lastRect;
        
            public override float GetHeight()
            {
                if(lastContent == null) lastContent = new GUIContent(helpBox.helpText);
                var h                               = EditorStyles.helpBox.CalcHeight(lastContent, lastRect.width * 0.75f);
                return h * 1.25f;
            }

            public override void OnGUI(Rect position)
            {
                lastRect = position;
                UnityEditor.MessageType t = default;
                switch (helpBox.type)
                {
                    case MessageType.None:
                        t = UnityEditor.MessageType.None;
                        break;
                    case MessageType.Info: 
                        t = UnityEditor.MessageType.Info;
                        break;
                    case MessageType.Warning: 
                        t = UnityEditor.MessageType.Warning;
                        break;
                    case MessageType.Error: 
                        t = UnityEditor.MessageType.Error;
                        break;
                }
            
                EditorGUI.HelpBox(position, helpBox.helpText, t);
            }
        }
    }