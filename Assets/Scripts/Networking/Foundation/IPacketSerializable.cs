using System;
using Carrom;
using UnityEngine;

namespace Networking.Foundation
{
    public interface IPacketSerializable
    {
        void ReadFromPacket(Packet packet);
        void WriteToPacket(Packet  packet);
    }

    public struct MatchmakeRequestData : IPacketSerializable, IEquatable<MatchmakeRequestData>
    {
        public int Bet;
        public int PlayerCount;

        public void ReadFromPacket(Packet packet)
        {
            Bet         = packet.ReadInt();
            PlayerCount = packet.ReadInt();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(Bet);
            packet.Write(PlayerCount);
        }

        public bool Equals(MatchmakeRequestData other)
        {
            return other.Bet == this.Bet && other.PlayerCount == this.PlayerCount;
        }
    }

    public struct MatchmakeResultData : IPacketSerializable
    {
        public string Auth;
        public string MatchServerAddress;
        public int    MatchServerPort;

        public void ReadFromPacket(Packet packet)
        {
            Auth               = packet.ReadString();
            MatchServerAddress = packet.ReadString();
            MatchServerPort    = packet.ReadInt();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(Auth);
            packet.Write(MatchServerAddress);
            packet.Write(MatchServerPort);
        }

        public override string ToString()
        {
            return $"Auth:{Auth}\tIP:{MatchServerAddress}\tPort:{MatchServerPort}";
        }
    }

    public struct AcceptMatch : IPacketSerializable
    {
        public string Auth;

        public void ReadFromPacket(Packet packet)
        {
            Auth = packet.ReadString();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(Auth);
        }
    }

    public struct MatchData : IPacketSerializable
    {
        public int          PlayerCount;
        public string       MatchUniqueId;
        public PlayerNumber YourPlayerNumber;
        public string[]     PlayerNames;
        public string[]     AvatarURIs;

        public void ReadFromPacket(Packet packet)
        {
            PlayerCount      = packet.ReadInt();
            MatchUniqueId    = packet.ReadString();
            YourPlayerNumber = (PlayerNumber) packet.ReadInt();
        
            PlayerNames = new string[PlayerCount];
            AvatarURIs  = new string[PlayerCount];

            for (int i = 0; i < PlayerCount; i++)
            {
                PlayerNames[i] = packet.ReadString();
                AvatarURIs[i]  = packet.ReadString();
            }
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(PlayerCount);
            packet.Write(MatchUniqueId);
            packet.Write((int) YourPlayerNumber);

            for (int i = 0; i < PlayerCount; i++)
            {
                packet.Write(PlayerNames[i]);
                packet.Write(AvatarURIs[i]);
            }
        }
    }

    public struct SubmitTurnData : IPacketSerializable
    {
        public string Auth;
        public float  Baseline01;
        public float  AngleRad;

        public void ReadFromPacket(Packet packet)
        {
            Auth       = packet.ReadString();
            Baseline01 = packet.ReadFloat();
            AngleRad   = packet.ReadFloat();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(Auth);
            packet.Write(Baseline01);
            packet.Write(AngleRad);
        }
    }
    
    public struct TurnStartData : IPacketSerializable
    {
        public PlayerNumber PlayerNumber;
        public float        Baseline01;
        public float        AngleRad;
        public float        Force;

        public void ReadFromPacket(Packet packet)
        {
            PlayerNumber = (PlayerNumber) packet.ReadInt();
            Baseline01   = packet.ReadFloat();
            AngleRad     = packet.ReadFloat();
            Force        = packet.ReadFloat();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write((int)PlayerNumber);
            packet.Write(Baseline01);
            packet.Write(AngleRad);
            packet.Write(Force);
        }
    }

    public struct TurnEndData : IPacketSerializable
    {
        public const int MaxCmEachColor = 8;

        public int turnId;
        
        public CarromGameplay.PlayerProgress[] Progresses;
        public PlayerNumber                    AwaitingTurn;
        public bool                            CoverStroke;
        public Vector3[]                       WhiteCmPositions;
        public Vector3[]                       BlackCmPositions;
        public Vector3                         QueenCmPosition;

        public void ReadFromPacket(Packet packet)
        {
            turnId = packet.ReadInt();
            int playerCount = packet.ReadInt();
            Progresses = new CarromGameplay.PlayerProgress[playerCount];
            for (int i = 0; i < playerCount; i++)
            {
                Progresses[i] = new CarromGameplay.PlayerProgress();
                Progresses[i].ReadFromPacket(packet);
            }

            AwaitingTurn = (PlayerNumber)packet.ReadInt();
            CoverStroke  = packet.ReadBool();

            WhiteCmPositions = new Vector3[MaxCmEachColor];
            for (int i = 0; i < MaxCmEachColor; i++)
            {
                WhiteCmPositions[i] = packet.ReadVector3();
            }
            
            BlackCmPositions = new Vector3[MaxCmEachColor];
            for (int i = 0; i < MaxCmEachColor; i++)
            {
                BlackCmPositions[i] = packet.ReadVector3();
            }

            QueenCmPosition = packet.ReadVector3();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(turnId);
            packet.Write(Progresses.Length);
            for (int i = 0; i < Progresses.Length; i++)
            {
                Progresses[i].WriteToPacket(packet);
            }

            packet.Write((int)AwaitingTurn);
            packet.Write(CoverStroke);

            for (int i = 0; i < MaxCmEachColor; i++)
            {
                packet.Write(WhiteCmPositions[i]);
            }
            
            for (int i = 0; i < MaxCmEachColor; i++)
            {
                packet.Write(BlackCmPositions[i]);
            }

            packet.Write(QueenCmPosition);
        }
    }

    public struct MatchResolutionData : IPacketSerializable
    {
        public string                          MatchUniqueId;
        public CarromGameplay.PlayerProgress[] Progresses;
        public PlayerNumber                    Winner;

        public void ReadFromPacket(Packet packet)
        {
            MatchUniqueId = packet.ReadString();
            int playerCount = packet.ReadInt();
            for (int i = 0; i < playerCount; i++)
            {
                Progresses[i] = new CarromGameplay.PlayerProgress();
                Progresses[i].ReadFromPacket(packet);
            }

            Winner = (PlayerNumber)packet.ReadInt();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(MatchUniqueId);
            packet.Write(Progresses.Length);
            for (int i = 0; i < Progresses.Length; i++)
            {
                Progresses[i].WriteToPacket(packet);
            }
        
            packet.Write((int)Winner);
        }
    
    }
}