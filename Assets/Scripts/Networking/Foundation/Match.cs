using System.Collections.Generic;
using Carrom;
using HarshCommon.NetworkStream;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

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

        #region Events

        [System.Serializable]
        public class MatchDataReceivedEvent : UnityEvent<MatchData>
        {
        }

        public MatchDataReceivedEvent onMatchDataReceived;

        [System.Serializable]
        public class MatchResolvedEvent : UnityEvent<MatchResolutionData>
        {
        }

        public MatchResolvedEvent onMatchResolved;

        #endregion

        #region Sometimes-Available Singleton

        public static Match Instance;

        public void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            else
            {
                Instance = this;
            }

            SubscribeToNetworkStream();

            void SubscribeToNetworkStream()
            {
                NetworkStream<MatchmakeResultData>.OnStreamHasItems += AcceptMatchAndSetup;
                NetworkStream<MatchData>.OnStreamHasItems           += ReceiveMatchData;
                NetworkStream<TurnStartData>.OnStreamHasItems       += PlayTurn;
                NetworkStream<TurnEndData>.OnStreamHasItems         += TurnEnd;
                NetworkStream<MatchResolutionData>.OnStreamHasItems += ResolveMatch;
            }
        }

        public void OnDisable()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            UnsubscribeToNetworkStream();

            void UnsubscribeToNetworkStream()
            {
                NetworkStream<MatchmakeResultData>.OnStreamHasItems -= AcceptMatchAndSetup;
                NetworkStream<MatchData>.OnStreamHasItems           -= ReceiveMatchData;
                NetworkStream<TurnStartData>.OnStreamHasItems       -= PlayTurn;
                NetworkStream<TurnEndData>.OnStreamHasItems         -= TurnEnd;
                NetworkStream<MatchResolutionData>.OnStreamHasItems -= ResolveMatch;
            }
        }

        #endregion

        #region Match Setup

        public void AcceptMatchAndSetup(IEnumerable<NetworkStream<MatchmakeResultData>.DataWrapper> dataWrappers)
        {
            foreach (var data in dataWrappers)
            {
                if (data.IsNotExpired && !isInMatch)
                {
                    _auth = data.Data.Auth;

                    Client.Instance.match = Client.Instance.tcp;
                    AcceptFoundMatch(Client.Instance.match);

                    data.MarkForRemoval();
                }
            }
            /*Client.Instance.EnterMatch(matchmakeResult.MatchServerAddress, matchmakeResult.MatchServerPort);
            Subscribe(Client.Instance.match); todo */

            void AcceptFoundMatch(Client.TCP tcp)
            {
                var acceptMatch = new AcceptMatch();
                acceptMatch.Auth = Auth;
                ClientSend.AcceptMatch(ref acceptMatch);

                Debug.Log($"Accepting Match.");
            }

            void ErrorConnectingToMatchServer(Client.TCP tcp)
            {
                Debug.Log(
                    $"Did find match, but encountered error in connecting to Match Server: {tcp.socket.Client.RemoteEndPoint}");
            }
        }

        public void ReceiveMatchData(IEnumerable<NetworkStream<MatchData>.DataWrapper> dataWrappers)
        {
            foreach (var matchData in dataWrappers)
            {
                if (matchData.IsNotExpired && !isInMatch)
                {
                    PlayerCount = matchData.Data.PlayerCount;
                    _matchData  = matchData.Data;
                    _isInMatch  = true;

                    gameplay.BeginGame(this);

                    onMatchDataReceived.Invoke(matchData.Data);
                }

                matchData.MarkForRemoval();
            }
        }

        #endregion

        #region Gameplay

        public void SubmitShot(float baseline01, float angleRad, float forceFactor)
        {
            var submitData = new SubmitTurnData();
            submitData.Auth        = Auth;
            submitData.Baseline01  = baseline01;
            submitData.AngleRad    = angleRad;
            submitData.ForceFactor = forceFactor;

            ClientSend.SubmitTurn(ref submitData);
        }

        #endregion

        #region Handle Network Events

        public static readonly AnimationControlSemaphore AnimationControl = new AnimationControlSemaphore(1);

        public void PlayTurn(IEnumerable<NetworkStream<TurnStartData>.DataWrapper> dataWrappers)
        {
            foreach (var turnStart in dataWrappers)
            {
                if (turnStart.IsNotExpired && isInMatch && gameplay.CanStartTurn(turnStart.Data.TurnId))
                {
                    Debug.Log($"Turn start received from server.");
                    gameplay.StartCoroutine(gameplay.TryDoTurn(turnStart.Data, () => turnStart.MarkForRemoval(), null));
                }
            }
        }

        public void TurnEnd(IEnumerable<NetworkStream<TurnEndData>.DataWrapper> dataWrappers)
        {
            foreach (var turnEnd in dataWrappers)
            {
                if (turnEnd.IsNotExpired && isInMatch && gameplay.CanEndTurn(turnEnd.Data.turnId))
                {
                    Debug.Log($"Turn End received from Server.");
                    gameplay.LoadStateFromPendingTurn(turnEnd.Data);
                    turnEnd.MarkForRemoval();
                }
            }
        }

        public void ResolveMatch(IEnumerable<NetworkStream<MatchResolutionData>.DataWrapper> dataWrappers)
        {
            foreach (var resolution in dataWrappers)
            {
                Debug.Log($"Match resolved. {resolution.Data.Winner.ToString()}");
                Client.Instance.match.Disconnect();
                // todo -- proper resolution

                resolution.MarkForRemoval();

                onMatchResolved?.Invoke(resolution.Data);
            }
        }

        #endregion
    }
}