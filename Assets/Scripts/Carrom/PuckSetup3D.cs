using UnityEngine;

namespace Carrom
{
    public class PuckSetup3D : MonoBehaviour
    {
        public Rigidbody rb;
        public bool      isStriker = false;

        public        float physicsScale = 1.0f;

        public void Awake()
        {
            PuckSetup.PhysicsScale = Mathf.Max(physicsScale, PuckSetup.PhysicsScale);
        }

        public void Start()
        {
            if (rb == null) return;
            var diameter  = isStriker ? BoardMeasurements.StrikerDiameter : BoardMeasurements.CarrommenDiameter;
            var thickness = isStriker ? BoardMeasurements.StrikerThickness : BoardMeasurements.CarrommenThickness;
            var mass      = isStriker ? BoardMeasurements.StrikerWeight : BoardMeasurements.CarrommenWeight;

            rb.mass     = mass * PuckSetup.PhysicsScale;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.drag     = 0.0f;
            rb.useGravity = true;
            
            var scaledDiameter = diameter * (isStriker ? PuckSetup.PhysicsScale : 1);

            transform.localScale = new Vector3(scaledDiameter, scaledDiameter, thickness);
        }
    }
}