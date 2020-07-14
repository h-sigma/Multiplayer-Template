using TMPro;
using UnityEngine.Events;

namespace Networking.Foundation
{
    public class ClientTestUIHandler : Singleton<ClientTestUIHandler>
    {
        public TextMeshProUGUI usernameField;
        public UnityEvent onConnect;
        
        public void OnConnectClick()
        {
            Client.Instance.tcp.OnConnect += (tcp) =>
            {
                onConnect?.Invoke();
            };
            Client.Instance.ConnectToServer();
        }

    }
}
