using System.Net;
using Carrom;
using UnityEngine;
using UnityEngine.Assertions;

namespace Networking.Foundation
{
    public static class ClientHandle
    {
        public static void Message(Packet packet)
        {
            string msg = packet.ReadString();
            Debug.Log($"Message received from server: {msg}");
        }

        public static void Welcome(Packet packet)
        {
            string msg = packet.ReadString();
            int myId = packet.ReadInt();
            
            Debug.Log($"Message from server: {msg}");
            Client.Instance.myId = myId;
            ClientSend.WelcomeReceived();
            
            Client.Instance.udp.Connect(((IPEndPoint) Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void MatchmakeResult(Packet packet)
        {
            MatchmakeResultData data = new MatchmakeResultData();
            data.ReadFromPacket(packet);
            
            Debug.Log($"Matchmake result received. {data.ToString()}");
            
            Assert.IsNotNull(Matchmaker.Instance);
            Matchmaker.Instance.FoundMatch(data);
        }

        public static void MatchDataReceive(Packet packet)
        {
            var matchData = new MatchData();
            matchData.ReadFromPacket(packet);

            Assert.IsNotNull(Match.Instance);
            if (Match.Instance != null)
            {
                Match.Instance.ReceiveMatchData(ref matchData);
            }
        }
        
        public static void TurnStartReceive(Packet packet)
        {
            var turnStart = new TurnStartData();
            turnStart.ReadFromPacket(packet);
            
            TurnStartReceive(ref turnStart);
        }

        public static void TurnStartReceive(ref TurnStartData turnStart)
        {
            Assert.IsNotNull(Match.Instance);
            if (Match.Instance != null)
            {
                Match.Instance.PlayTurn(ref turnStart);
            }
        }

        public static void TurnEndReceive(Packet packet)
        {
            var turnEnd = new TurnEndData();
            turnEnd.ReadFromPacket(packet);
            
            Assert.IsNotNull(Match.Instance);
            if (Match.Instance != null)
            {
                Match.Instance.TurnEnd(ref turnEnd);
            }
        }

        public static void MatchResolutionReceived(Packet packet)
        {
            var matchResolution = new MatchResolutionData();
            matchResolution.ReadFromPacket(packet);
            
            Assert.IsNotNull(Match.Instance);
            if (Match.Instance != null)
            {
                Match.Instance.ResolveMatch(ref matchResolution);
            }
        }
        
        public static void SyncGameplayConstants(Packet packet)
        {
            var gameplayConstants = new CarromGameplayHelper.ServerGameplayConstants();
            gameplayConstants.ReadFromPacket(packet);
            
            CarromGameplayHelper.LoadGameplayConstants(gameplayConstants);
        }

        /*
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
        }*/
    }
}