using Carrom;
using UnityEngine;
using UnityEngine.Assertions;

namespace Networking.Foundation
{
    public class Match : MonoBehaviour
    {
        #region Inspector

        public CarromGameplay gameplay = default;

        #endregion

        #region Properties

        private string _auth = string.Empty;
        public  string Auth => _auth;

        private MatchData _matchData;
        public  MatchData MatchData => _matchData;

        private bool _isInMatch = false;
        public  bool isInMatch => _isInMatch;

        public int PlayerCount { get; private set; }

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

            AcceptMatchAndSetup();
        }

        public void OnDisable()
        {
            if (Instance == this)
                Instance = null;
        }

        #endregion

        #region Match Setup

        public void AcceptMatchAndSetup()
        {
            var matchmakeResult = Matchmaker.Instance.CurrentMatchmakeResultData;
            /*Client.Instance.EnterMatch(matchmakeResult.MatchServerAddress, matchmakeResult.MatchServerPort);
            Subscribe(Client.Instance.match); todo */
            Client.Instance.match = Client.Instance.tcp;
            AcceptFoundMatch(Client.Instance.match);

            void AcceptFoundMatch(Client.TCP tcp)
            {
                var acceptMatch = new AcceptMatch();
                acceptMatch.Auth = Auth;
                ClientSend.AcceptMatch(ref acceptMatch);

                Debug.Log($"Accepting Match.");

                Unsubscribe(tcp);
            }

            void ErrorConnectingToMatchServer(Client.TCP tcp)
            {
                Unsubscribe(tcp);
                Debug.Log(
                    $"Did find match, but encountered error in connecting to Match Server: {tcp.socket.Client.RemoteEndPoint}");
            }

            /*
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
            */
        }

        public void ReceiveMatchData(ref MatchData matchData)
        {
            PlayerCount = matchData.PlayerCount;
            _matchData = matchData;
            _isInMatch = true;

            gameplay.BeginGame(this);
        }

        #endregion

        #region Gameplay

        public void SubmitShot(float baseline01, float angleRad)
        {
            var submitData = new SubmitTurnData();
            submitData.Auth       = Auth;
            submitData.Baseline01 = baseline01;
            submitData.AngleRad   = angleRad;

            ClientSend.SubmitTurn(ref submitData);
        }

        #endregion

        public void PlayTurn(ref TurnStartData turnStart)
        {
            Debug.Log(
                $"Wanting to play turn Base:{turnStart.Baseline01}\tForce:{turnStart.Force}\tAngle:{turnStart.AngleRad}\tPlayer:{turnStart.PlayerNumber}");

            gameplay.TryDoTurn(turnStart);
        }

        public void TurnEnd(ref TurnEndData turnEnd)
        {
            Debug.Log($"Turn End received from Server.");

            gameplay.LoadStateFromPendingTurn(turnEnd);
        }

        public void ResolveMatch(ref MatchResolutionData matchResolution)
        {
            Debug.Log($"Match resolved. {matchResolution.Winner.ToString()}");
            Client.Instance.match.Disconnect();
        }
    }
}