using HarshCommon.Networking;
using Networking.Foundation;

namespace Carrom
{
    public class CarromGameplayHelper
    {
        //todo -- get server gameplay constants through network
        public class ServerGameplayConstants : IPacketSerializable
        {
            public float Force = 0.047f;
            
            public void ReadFromPacket(Packet packet)
            {
                Force = packet.ReadFloat();
            }

            public void WriteToPacket(Packet packet)
            {
                packet.Write(Force);
            }
        }

        private static ServerGameplayConstants _gameplayConstants = new ServerGameplayConstants();

        public static void LoadGameplayConstants(ServerGameplayConstants constants)
        {
            _gameplayConstants = constants;
        }
        
        public static ServerGameplayConstants GetServerConstants()
        {
            return _gameplayConstants;
        }
    }
}