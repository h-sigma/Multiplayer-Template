using UnityEngine.Assertions;

namespace Networking.Foundation
{
    public class ClientSend
    {
        private static void SendTCPData(Packet packet, Client.TCP tcp = default)
        {
            packet.WriteLength();
            if (tcp == default)
            {
                Assert.IsNotNull(Client.Instance.tcp);
                Client.Instance.tcp.SendData(packet);
            }
            else
            {
                tcp.SendData(packet);
            }
        }

        private static void SendUDPData(Packet packet, Client.UDP udp = default)
        {
            packet.WriteLength();
            if (udp == default)
            {
                Client.Instance.udp.SendData(packet);
            }
            else
            {
                udp.SendData(packet);
            }
        }

        #region Packets

        public static void SendMessage(string message)
        {
            using (var packet = new Packet((int) ClientPackets.message))
            {
                packet.Write(message);
                //SendUDPData(packet);
                SendTCPData(packet);
            }
        }

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

        /*public static void PlayerMovement(bool[] inputs)
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
        }*/

        public static void SendStruct<TStruct>(ref TStruct tstruct, ClientPackets packetId, Client.TCP tcp)
            where TStruct : struct, IPacketSerializable
        {
            using (var packet = new Packet((int) packetId))
            {
                tstruct.WriteToPacket(packet);
                SendTCPData(packet, tcp);
            }
        }

        public static void MatchmakeRequest(ref MatchmakeRequestData matchmakeRequestData)
        {
            SendStruct(ref matchmakeRequestData, ClientPackets.matchmake, null);
        }

        public static void AcceptMatch(ref AcceptMatch accept)
        {
            Assert.IsNotNull(Client.Instance.match);
            SendStruct(ref accept, ClientPackets.acceptMatch, Client.Instance.match);
        }

        public static void SubmitTurn(ref SubmitTurnData submitTurnData)
        {
            Assert.IsNotNull(Client.Instance.match);
            SendStruct(ref submitTurnData, ClientPackets.submitTurn, Client.Instance.match);

            /* todo -- check delay of turn start arriving from server
            Assert.IsNotNull(Match.Instance);
            var turnStart = new TurnStartData();
            turnStart.Baseline01 = submitTurnData.Baseline01;
            turnStart.AngleRad = submitTurnData.AngleRad;
            turnStart.PlayerNumber = Match.Instance.MatchData.PlayerNumber;
            turnStart.Force = CarromGameplayHelper.GetServerConstants().Force;
            
            ClientHandle.TurnStartReceive(ref turnStart);*/
        }

        public static void Forfeit(string Auth)
        {
            using (var packet = new Packet((int) ClientPackets.forfeit))
            {
                packet.Write(Auth);
                if (Client.Instance.match.socket.Connected)
                {
                    SendTCPData(packet, Client.Instance.match);
                }
                else
                {
                    SendTCPData(packet);
                }
            }
        }
    }
}