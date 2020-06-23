using UnityEngine;

namespace Utility
{
    public enum MessageType
    {
        None, Info, Warning, Error
    }
    public class HelpBoxAttribute : PropertyAttribute
    {
        public string      helpText;
        public MessageType type;

        public HelpBoxAttribute(string text, Utility.MessageType t = MessageType.Info)
        {
            helpText = text;
            type     = t;
        }
    }
}