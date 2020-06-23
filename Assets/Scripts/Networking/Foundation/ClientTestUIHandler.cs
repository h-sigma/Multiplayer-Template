using TMPro;
using UnityEngine;

namespace Networking.Foundation
{
    public class ClientTestUIHandler : Singleton<ClientTestUIHandler>
    {
        public TextMeshProUGUI usernameField;
        
        public void OnConnectClick()
        {
            Client.Instance.ConnectToServer();
            var canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }

    }
}
