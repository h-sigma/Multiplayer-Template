using System;
using UnityEngine;

namespace Utility
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValidationAttribute : PropertyAttribute
    {
        public StringOperations Operations;
        public int              MaxLength;
        public bool             TextArea;
        public char[]           Trim;

        public StringValidationAttribute(StringOperations operations, int maxLength = -1, bool textArea = false,
            char[]                                        trim = null)
        {
            this.Operations = operations;
            this.MaxLength  = maxLength;
            this.TextArea   = textArea;
            this.Trim       = trim;
        }
    }

    [Flags]
    public enum StringOperations
    {
        MaxLength = 1 << 0,
        Upper     = 1 << 1,
        Lower     = 1 << 2,
        Trim      = 1 << 3
    }
}