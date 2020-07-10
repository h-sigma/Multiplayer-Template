using UnityEngine;

namespace Carrom
{
    public class PuckSetup : MonoBehaviour
    {
        public Rigidbody2D     rb;
        public FrictionJoint2D joint;
        public bool            isStriker = false;

        public        float gravity               = 9.8f;
        public        float coefficientOfFriction = 0.25f;
        public        float physicsScale          = 1.0f;
        public static float PhysicsScale          = Mathf.NegativeInfinity;

        public void Awake()
        {
            //Assert.IsNotNull(rb);
            //Assert.IsNotNull(joint);

            PhysicsScale = Mathf.Max(physicsScale, PhysicsScale);
        }

        public void Start()
        {
            if (rb == null) return;
            var diameter  = isStriker ? BoardMeasurements.StrikerDiameter : BoardMeasurements.CarrommenDiameter;
            var thickness = isStriker ? BoardMeasurements.StrikerThickness : BoardMeasurements.CarrommenThickness;
            var mass      = isStriker ? BoardMeasurements.StrikerWeight : BoardMeasurements.CarrommenWeight;

            var friction = mass * gravity * coefficientOfFriction;

            rb.mass     = mass * PhysicsScale;
            rb.drag     = 0.0f;
            rb.bodyType = RigidbodyType2D.Dynamic;

            joint.maxForce = friction * PhysicsScale;

            var scaledDiameter = diameter * (isStriker ? PhysicsScale : 1);

            transform.localScale = new Vector3(scaledDiameter, scaledDiameter, thickness);
        }
    }
}