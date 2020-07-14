using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace Networking.Foundation
{
    public enum RuleSet
    {
        Standard,
        FreeForAll
    }

    [System.Serializable]
    public class GameConfig
    {
        public RuleSet ruleSet;
        public int     playerCount;
        public int     bet;
    }

    public class Matchmaker : Singleton<Matchmaker>
    {
        //todo -- cleanup code to non-mono
        public GameConfig config;

        public SceneTransition toMatch;

        private bool lookingForMatches = false;

        public bool LookingForMatches => lookingForMatches;

        private MatchmakeResultData _currentMatchmakeResultData;
        public MatchmakeResultData CurrentMatchmakeResultData
        {
            get => _currentMatchmakeResultData;
            private set => _currentMatchmakeResultData = value;
        }

        [Serializable]
        public class MatchmakeResultEvent : UnityEvent<MatchmakeResultData>
        {
        }

        public MatchmakeResultEvent onMatchFound;
        public UnityEvent           onMatchNotFound;
        
        public void BeginMatchmake()
        {
            var matchmakeRequestData = new MatchmakeRequestData {Bet = config.bet, PlayerCount = config.playerCount};

            ClientSend.MatchmakeRequest(ref matchmakeRequestData);

            onMatchFound.AddListener(OnMatchmakeSuccess);
            onMatchNotFound.AddListener(OnMatchmakeFail);

            lookingForMatches = true;

            void OnMatchmakeSuccess(MatchmakeResultData result)
            {
                //todo -- should be returned through network
                CurrentMatchmakeResultData = result;
                toMatch.TryTransition(true);
            }

            void OnMatchmakeFail()
            {
                Debug.Log($"Failed to matchmake.");
            }
        }

        public void FoundMatch(MatchmakeResultData data)
        {
            if (string.IsNullOrEmpty(data.Auth))
            {
                onMatchNotFound?.Invoke();
            }
            else
            {
                onMatchFound?.Invoke(data);
            }
        }
    }
}