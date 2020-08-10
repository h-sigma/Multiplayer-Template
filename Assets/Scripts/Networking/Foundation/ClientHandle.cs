using System.Net;
using Carrom;
using HarshCommon.Networking;
using HarshCommon.NetworkStream;
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

        //todo -- remove all instant responses

        public static void ForwardToNetworkStream<T>(Packet packet) where T : struct, IPacketSerializable
        {
            NetworkStream<T>.Enqueue(packet);
        }

        /*
         public static void MatchDataReceive(Packet packet)
        {
            if (Match.Instance != null)
            {
                var matchData = new MatchData();
                matchData.ReadFromPacket(packet);
                Match.Instance.ReceiveMatchData(ref matchData);
                NetworkStream<MatchData>.Enqueue(ref matchData);
            }
            else
            {
                NetworkStream<MatchData>.Enqueue(packet);
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
            if (Match.Instance != null)
            {
                Match.Instance.PlayTurn(turnStart);
                NetworkStream<TurnStartData>.Enqueue(ref turnStart);
            }
            else
            {
                NetworkStream<TurnStartData>.Enqueue(ref turnStart);
            }
        }

        public static void TurnEndReceive(Packet packet)
        {
            if (Match.Instance != null)
            {
                var turnEnd = new TurnEndData();
                turnEnd.ReadFromPacket(packet);
                Match.Instance.TurnEnd(ref turnEnd);
                NetworkStream<TurnEndData>.Enqueue(ref turnEnd);
            }
            else
            {
                NetworkStream<TurnEndData>.Enqueue(packet);
            }
        }

        public static void MatchResolutionReceived(Packet packet)
        {
            if (Match.Instance != null)
            {
                var matchResolution = new MatchResolutionData();
                matchResolution.ReadFromPacket(packet);
                Match.Instance.ResolveMatch(ref matchResolution);
                NetworkStream<MatchResolutionData>.Enqueue(ref matchResolution);
            }
            else
            {
                NetworkStream<MatchResolutionData>.Enqueue(packet);
            }
        }
        */
        
        public static void SyncGameplayConstants(Packet packet)
        {
            var gameplayConstants = new CarromGameplayHelper.ServerGameplayConstants();
            gameplayConstants.ReadFromPacket(packet);
            
            CarromGameplayHelper.LoadGameplayConstants(gameplayConstants);
        }
    }
}