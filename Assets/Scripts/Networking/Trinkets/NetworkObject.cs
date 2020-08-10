using UnityEngine;

namespace Networking.Trinkets
{
    public class NetworkObject : MonoBehaviour
    {
        public int networkId;

        public void SyncTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            var transform1 = transform;
            transform1.localPosition = position;
            transform1.localRotation = rotation;
            transform1.localScale = scale;
        }
    }
}