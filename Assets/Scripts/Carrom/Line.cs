using System;
using UnityEngine;

namespace Carrom
{
    [Serializable]
    public class Line
    {
        public Transform left;
        public Transform right;

        public Vector3 GetPosition(float baseline01)
        {
            return Vector3.Lerp(left.localPosition, right.localPosition, baseline01);
        }

        public Vector3 Right => (right.localPosition - left.localPosition).normalized;

        public Vector3 Left => -Right;

        public Vector3 Forward
        {
            get
            {
                var leftToRight = Right;
                var forward     = new Vector3(-leftToRight.y, leftToRight.x, leftToRight.z);
                return forward;
            }
        }

        public Vector3 Back => -Forward;
    }
}