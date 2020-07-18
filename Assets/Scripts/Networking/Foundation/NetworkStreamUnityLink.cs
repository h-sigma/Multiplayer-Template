using UnityEngine;

namespace Networking.Foundation
{
    public class NetworkStreamUnityLink : MonoBehaviour
    {
        public bool running = true;
        public float updatesPerSecond = 30;

        private float _timeLastUpdate;

        public void Start()
        {
            _timeLastUpdate = Time.time;
        }

        public void Update()
        {
            if (running && _timeLastUpdate + 1.0f / updatesPerSecond > Time.time)
            {
                _timeLastUpdate = Time.time;
                NetworkStreamController.UpdateStreams();
            }
        }
    }
}