using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Assertions;

namespace Networking.Foundation
{
    public class Client : Singleton<Client>
    {
        public static readonly int BUFFER_SIZE = 4096;

        public string serverIp = "127.0.0.1";
        public int    port     = 26950;
        public int    myId     = 0;

        public TCP tcp;
        public UDP udp;

        public TCP match;

        public event Action OnEnteredMatch;

        private bool isConnected = false;
        private bool isInMatch = false;

        private delegate void PacketHandler(Packet packet);

        private static Dictionary<int, PacketHandler> _packetHandlers;

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            Disconnect();
        }

        private void Start()
        {
            tcp = new TCP();
            udp = new UDP();
            
            match = new TCP();
        }

        public void ConnectToServer()
        {
            tcp = tcp ?? new TCP();
            udp = udp ?? new UDP();

            InitializeClientData();
            isConnected = true;
            tcp.Connect(Instance.serverIp, Instance.port);
        }

        public void EnterMatch(string _ip, int _port)
        {
            Assert.IsFalse(isInMatch);
            if (_ip == "0.0.0.0") _ip = "127.0.0.1";
            if (!isInMatch)
            {
                match.Connect(_ip, _port);
                isInMatch = true;
            }
        }

        public void ExitMatch()
        {
            Assert.IsTrue(isInMatch);
            if(isInMatch)
            {
                if(match != tcp)
                {
                    match.Disconnect();
                }
                isInMatch = false;
            }
        }

        public void DebugMessage(string msg)
        {
            ClientSend.SendMessage(msg);
        }

        public class TCP
        {
            public TcpClient socket;

            public event Action<TCP> OnConnect;
            public event Action<TCP> OnDisconnectBeforeReset;

            private NetworkStream stream;

            private Packet receivedPacket;

            private byte[] receiveBuffer;

            public void Connect(string ip, int port)
            {
                socket = new TcpClient() {ReceiveBufferSize = BUFFER_SIZE, SendBufferSize = BUFFER_SIZE};

                receiveBuffer = new byte[BUFFER_SIZE];

                socket.BeginConnect(ip, port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);

                if (!socket.Connected) return;

                stream = socket.GetStream();

                receivedPacket = new Packet();

                stream.BeginRead(receiveBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);
                
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    OnConnect?.Invoke(this);
                });
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = stream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Instance.Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivedPacket.Reset(HandleData(data));

                    stream.BeginRead(receiveBuffer, 0, BUFFER_SIZE, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Debug.Log($"Failed to receive packet from TCP : {e}");
                    Disconnect();
                }
            }

            public void SendData(Packet packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error sending data to server via TCP: {e}");
                }
            }

            /// <summary>
            /// Handles data and manages packets that arrive in portions.
            /// </summary>
            /// <returns></returns>
            private bool HandleData(byte[] data)
            {
                int packetLength = 0;

                receivedPacket.SetBytes(data);

                if (receivedPacket.UnreadLength() >= Packet.INT_SIZE)
                {
                    packetLength = receivedPacket.ReadInt();
                    if (packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
                {
                    byte[] packetBytes = receivedPacket.ReadBytes(packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (var packet = new Packet(packetBytes))
                        {
                            int id = packet.ReadInt();
                            _packetHandlers[id](packet);
                        }
                    });

                    packetLength = 0;
                    if (receivedPacket.UnreadLength() >= Packet.INT_SIZE)
                    {
                        packetLength = receivedPacket.ReadInt();
                        if (packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }

                if (packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                receivedPacket?.Dispose();
                
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    OnDisconnectBeforeReset?.Invoke(this);
                });

                stream         = null;
                receivedPacket = null;
                socket         = null;
                receiveBuffer  = null;
            }
        }

        public class UDP
        {
            public UdpClient  socket;
            public IPEndPoint endPoint;

            public UDP()
            {
                endPoint = new IPEndPoint(IPAddress.Parse(Instance.serverIp), Instance.port);
            }

            public void Connect(int localPort)
            {
                socket = new UdpClient(localPort);

                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                // todo -- ???
                using (var packet = new Packet())
                {
                    SendData(packet);
                }
            }

            public void SendData(Packet packet)
            {
                try
                {
                    packet.InsertInt(Instance.myId);
                    socket?.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
                catch (Exception e)
                {
                    Debug.Log($"Error sending data to server via UDP: {e}");
                }
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    byte[] data = socket.EndReceive(result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if (data.Length < Packet.INT_SIZE)
                    {
                        Instance.Disconnect();
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception e)
                {
                    Debug.Log($"Failed to receive UDP data from server: {e}");
                    Disconnect();
                }
            }

            private void HandleData(byte[] data)
            {
                using (var packet = new Packet(data))
                {
                    var packetLength = packet.ReadInt();
                    var smallerData  = packet.ReadBytes(packetLength);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (var pkt = new Packet(smallerData))
                        {
                            int pktId = pkt.ReadInt();
                            _packetHandlers[pktId].Invoke(pkt);
                        }
                    });
                }
            }

            public void Disconnect()
            {
                socket?.Close();
                
                endPoint = null;
                socket = null;
            }
        }

        private void InitializeClientData()
        {
            _packetHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int) ServerPackets.message, ClientHandle.Message},
                {(int) ServerPackets.welcome, ClientHandle.Welcome},
                {(int) ServerPackets.matchmakeResult, ClientHandle.MatchmakeResult},
                {(int) ServerPackets.matchdata, ClientHandle.MatchDataReceive},
                {(int) ServerPackets.turnStartData, ClientHandle.TurnStartReceive},
                {(int) ServerPackets.turnEndData, ClientHandle.TurnEndReceive},
                {(int) ServerPackets.matchResolution, ClientHandle.MatchResolutionReceived},
                {(int) ServerPackets.serverGameplayConstants, ClientHandle.SyncGameplayConstants},
            };

            Debug.Log("Initialized Packet Handlers.");
        }

        public void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                udp.Disconnect();
                tcp.Disconnect();
                
                match.Disconnect();

                Debug.Log("Disconnected from server.");
            }
        }
    }
}