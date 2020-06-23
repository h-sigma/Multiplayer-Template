using System.Net;
using UnityEngine;

namespace Networking.Foundation
{
    public static class ClientHandle
    {
        public static void Welcome(Packet packet)
        {
            string msg = packet.ReadString();
            int myId = packet.ReadInt();
            
            Debug.Log($"Message from server: {msg}");
            Client.Instance.myId = myId;
            ClientSend.WelcomeReceived();
            
            Client.Instance.udp.Connect(((IPEndPoint) Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void PlayerSpawn(Packet packet)
        {
            int id = packet.ReadInt();
            string username = packet.ReadString();
            Vector3 position = packet.ReadVector3();
            Quaternion rotation = packet.ReadQuaternion();
            
            GameManager.Instance.SpawnPlayer(id, username, position, rotation);
        }

        public static void PlayerPosition(Packet packet)
        {
            var id       = packet.ReadInt();
            var position = packet.ReadVector3();

            if (GameManager.players.TryGetValue(id, out var manager))
            {
                manager.SetPosition(position);
            }
        }

        public static void PlayerRotation(Packet packet)
        {
            var    id       = packet.ReadInt();
            var rotation = packet.ReadQuaternion();
            
            if (GameManager.players.TryGetValue(id, out var manager))
            {
                manager.SetRotation(rotation);
            }
        }
    }
}