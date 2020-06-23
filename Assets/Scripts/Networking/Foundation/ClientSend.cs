namespace Networking.Foundation
{
    public class ClientSend
    {
        private static void SendTCPData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.tcp.SendData(packet);
        }

        private static void SendUDPData(Packet packet)
        {
            packet.WriteLength();
            Client.Instance.udp.SendData(packet);
        }

        #region Packets

        public static void WelcomeReceived()
        {
            using (var packet = new Packet((int) ClientPackets.welcomeReceived))
            {
                packet.Write(Client.Instance.myId);
                packet.Write(ClientTestUIHandler.Instance.usernameField.text);
                
                SendTCPData(packet);
            }
        }
        
        
        #endregion

        public static void PlayerMovement(bool[] inputs)
        {
            using (var packet = new Packet((int) ClientPackets.playerMovement))
            {
                packet.Write(inputs.Length);
                for (int i = 0; i < inputs.Length; i++)
                {
                    packet.Write(inputs[i]);
                }
                packet.Write(GameManager.players[Client.Instance.myId].transform.rotation);
                
                SendUDPData(packet);
            }
        }
    }
}