using Carrom;
using UnityEngine;
using UnityEngine.Assertions;

namespace Networking.Foundation
{
    public class Match : MonoBehaviour
    {
        #region Inspector
        public        bool  keepTryingOnFail = false;
        public CarromGameplay gameplay  = default;
        #endregion

        #region Properties

        private string _auth = string.Empty;
        public string Auth => _auth;

        private MatchData _matchData;
        public MatchData MatchData => _matchData;

        private bool _isInMatch = false;
        public bool isInMatch => _isInMatch;
        
        #endregion

        #region Sometimes-Available Singleton
        
        public static Match Instance;
        public void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        public void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }
        #endregion

        #region Match Setup

        public void Start()
        {
            BeginMatch();
        }

        public void BeginMatch()
        {
            Matchmaker.Instance.TryMatchmaking(OnMatchmakeSuccess, OnMatchmakeFail);
            
            void OnMatchmakeSuccess(MatchmakeResultData result)
            {
                Subscribe(Client.Instance.match);
                this._auth = result.Auth;
                Client.Instance.EnterMatch(result.MatchServerAddress, result.MatchServerPort);

                void AcceptFoundMatch(Client.TCP tcp)
                {
                    var acceptMatch = new AcceptMatch();
                    acceptMatch.Auth = Auth;
                    ClientSend.AcceptMatch(ref acceptMatch);

                    Unsubscribe(tcp);
                }

                void ErrorConnectingToMatchServer(Client.TCP tcp)
                {
                    Unsubscribe(tcp);
                    Debug.Log($"Did find match, but encountered error in connecting to Match Server: {tcp.socket.Client.RemoteEndPoint}");
                }

                void Subscribe(Client.TCP tcp)
                {
                    Assert.IsNotNull(tcp);
                    tcp.OnConnect               += AcceptFoundMatch;
                    tcp.OnDisconnectBeforeReset += ErrorConnectingToMatchServer;
                }

                void Unsubscribe(Client.TCP tcp)
                {
                    Assert.IsNotNull(tcp);
                    tcp.OnConnect               -= AcceptFoundMatch;
                    tcp.OnDisconnectBeforeReset -= ErrorConnectingToMatchServer;
                }
            }
            
            void OnMatchmakeFail()
            {
                Debug.Log($"Failed to matchmake. {(keepTryingOnFail ? "Trying again." : "Not Trying again.")}");
                if (keepTryingOnFail)
                {
                    BeginMatch();
                }
            }
        }

        public void ReceiveMatchData(ref MatchData matchData)
        {
            _matchData = matchData;
            _isInMatch = true;

            gameplay.BeginGame(this);
        }
        
        #endregion
        
        #region Gameplay

        public void SubmitShot(float baseline01, float angleRad)
        {
            var submitData = new SubmitTurnData();
            submitData.Auth = Auth;
            submitData.Baseline01 = baseline01;
            submitData.AngleRad = angleRad;

            ClientSend.SubmitTurn(ref submitData);
        }
        
        #endregion

        public void PlayTurn(ref TurnStartData turnStart)
        {
            Debug.Log($"Wanting to play turn Base:{turnStart.Baseline01}\tForce:{turnStart.Force}\tAngle:{turnStart.AngleRad}\tPlayer:{turnStart.PlayerNumber}");
        }

        public void TurnEnd(ref TurnEndData turnEnd)
        {
            Debug.Log($"Turn End received from Server.");
        }

        public void ResolveMatch(ref MatchResolutionData matchResolution)
        {
            Debug.Log($"Match resolved. {matchResolution.Winner.ToString()}");
            Client.Instance.match.Disconnect();
        }
    }
}