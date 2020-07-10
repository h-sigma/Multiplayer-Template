using UnityEngine;

namespace Carrom.UI
{
    public abstract class SliderLerp : MonoBehaviour
    {
        public bool setZeroOnAwake = true;
        public abstract void SliderValueChanged(float value);
        
        public void Awake()
        {
            if(setZeroOnAwake)
                SliderValueChanged(0.0f);
        }
    }
}