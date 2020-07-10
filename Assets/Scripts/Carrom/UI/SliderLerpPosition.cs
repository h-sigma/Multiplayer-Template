using UnityEngine;

namespace Carrom.UI
{
    public class SliderLerpPosition : SliderLerp
    {
        public Transform from;
        public Transform to;
        public Transform controlled;

        public override void SliderValueChanged(float value)
        {
            controlled.localPosition = Vector3.Lerp(@from.position, to.position, value);
        }
    }
}