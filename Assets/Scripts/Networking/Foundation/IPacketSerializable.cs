using Carrom;
using UnityEngine;
using Utility;

namespace Networking.Foundation
{
    public interface IPacketSerializable
    {
        void ReadFromPacket(Packet packet);
        void WriteToPacket(Packet  packet);
    }


    public struct MatchmakeRequestData : IPacketSerializable
    {
        public int  Bet;
        public int PlayerCount;

        public void ReadFromPacket(Packet packet)
        {
            Bet       = packet.ReadInt();
            PlayerCount = packet.ReadInt();
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(Bet);
            packet.Write(PlayerCount);
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
        public const int MaxPossiblePlayers = 4;

        public string       MatchUniqueId;
        public PlayerNumber PlayerNumber;
        public string[]     PlayerNames;
        public string[]     AvatarURIs;
        public TokenColor[] Colors;

        public void ReadFromPacket(Packet packet)
        {
            MatchUniqueId = packet.ReadString();
            PlayerNumber  = (PlayerNumber) packet.ReadInt();

            PlayerNames = new string[MaxPossiblePlayers];
            AvatarURIs  = new string[MaxPossiblePlayers];
            Colors      = new TokenColor[MaxPossiblePlayers];

            for (int i = 0; i < MaxPossiblePlayers; i++)
            {
                PlayerNames[i] = packet.ReadString();
                AvatarURIs[i]  = packet.ReadString();
                Colors[i]      = (TokenColor) packet.ReadInt();
            }
        }

        public void WriteToPacket(Packet packet)
        {
            packet.Write(MatchUniqueId);
            packet.Write((int) PlayerNumber);

            for (int i = 0; i < MaxPossiblePlayers; i++)
            {
                packet.Write(PlayerNames[i]);
                packet.Write(AvatarURIs[i]);
                packet.Write((int) Colors[i]);
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
            packet.Write((int) PlayerNumber);
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
            for (int i = 0; i < MatchData.MaxPossiblePlayers; i++)
            {
                Progresses[i] = new CarromGameplay.PlayerProgress();
                Progresses[i].ReadFromPacket(packet);
            }

            AwaitingTurn = (PlayerNumber) packet.ReadInt();
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
            for (int i = 0; i < MatchData.MaxPossiblePlayers; i++)
            {
                Progresses[i].WriteToPacket(packet);
            }

            packet.Write((int) AwaitingTurn);
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
            for (int i = 0; i < MatchData.MaxPossiblePlayers; i++)
            {
                Progresses[i] = new CarromGameplay.PlayerProgress();
                Progresses[i].ReadFromPacket(packet);
            }

            Winner = (PlayerNumber) packet.ReadInt();
        }

        public void WriteToPacket(Packet packet)
        {
            throw new System.NotImplementedException();
        }
    }
}