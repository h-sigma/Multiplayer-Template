using UnityEngine;
using UnityEngine.Assertions;

namespace Carrom
{
    public class CameraSetup : MonoBehaviour
    {
        public new Camera camera;
        public void Awake()
        {
            Assert.IsNotNull(camera);
        }

        public void Start()
        {
            var physicsScale = PuckSetup.PhysicsScale;

            var side     = BoardMeasurements.PlayingSide;

            camera.orthographicSize = physicsScale * (side * 0.75f);
        }
    }
}