using UnityEngine;

namespace Networking.Foundation
{
    public class PlayerManager : MonoBehaviour
    {
        public int id;
        public string username;

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }
    }
}
