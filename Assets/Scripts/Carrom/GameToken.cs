using HarshCommon.Patterns.Registry;
using UnityEngine;

namespace Carrom
{
    [DefaultExecutionOrder(-100)]
    public class GameToken : SceneRegistryMonobehaviour<GameToken, GameTokenType>
    {
        [Header("Scene References")]
        public new Rigidbody rigidbody;
        public new Collider collider;

        private bool _isScored = false;
        public bool IsScored
        {
            get => _isScored;
            set
            {
                if (_isScored == value) return;
                _isScored = value;
            }
        }
        
        public void TemporaryRemoveFromSimulation()
        {
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.isKinematic = true;
            collider.enabled = false;
        }

        public void ReturnToSimulation()
        {
            rigidbody.isKinematic = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            collider.enabled = true;
        }
    }

    public enum GameTokenType
    {
        Queen,
        Black,
        White,
        Striker
    }
}