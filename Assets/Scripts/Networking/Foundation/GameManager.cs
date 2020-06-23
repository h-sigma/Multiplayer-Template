using System.Collections.Generic;
using UnityEngine;

namespace Networking.Foundation
{
    public class GameManager : Singleton<GameManager>
    {
        public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

        public GameObject localPlayerPrefab;
        public GameObject playerPrefab;
        
        public void Awake()
        {
            
        }
        
        public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
        {
            var player = Instantiate(_id == Client.Instance.myId ? localPlayerPrefab : playerPrefab, _position, _rotation);

            var manager = player.GetComponent<PlayerManager>();
            manager.id = _id;
            manager.username = _username;

            players.Add(_id, manager);
        }
    }
}
