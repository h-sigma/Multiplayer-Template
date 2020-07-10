using UnityEngine;
using UnityEngine.Assertions;

namespace Carrom
{
    public class BoardScaleSetup : MonoBehaviour
    {
        public Transform playspace;
        public Transform leftPuck;
        public Transform rightPuck;

        public void Awake()
        {
            Assert.IsNotNull(playspace);
        }

        public void Start()
        {
            var physicsScale = PuckSetup.PhysicsScale;
            
            playspace.transform.localScale = new Vector3(physicsScale, physicsScale, physicsScale);
        }
    }
}