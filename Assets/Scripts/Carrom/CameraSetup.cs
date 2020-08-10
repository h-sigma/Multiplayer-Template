using UnityEngine;
using UnityEngine.Assertions;

namespace Carrom
{
    public class CameraSetup : MonoBehaviour
    {
        public new Camera camera;
        
        public float sideFactor = 0.75f;
        
        public void Awake()
        {
            Assert.IsNotNull(camera);
        }

        public void Update()
        {
            var physicsScale = PuckSetup.PhysicsScale;

            var halfSide     = BoardMeasurements.PlayingSide / 2.0f;

            camera.orthographicSize = physicsScale * (halfSide * sideFactor) * (1 / camera.aspect);
        }
    }
}