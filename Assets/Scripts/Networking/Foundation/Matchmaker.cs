using System;
using System.Collections.Generic;
using HarshCommon.NetworkStream;
using HarshCommon.Patterns.Singleton;
using HarshCommon.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Networking.Foundation
{
    public enum RuleSet
    {
        Standard,
        FreeForAll
    }

    [Serializable]
    public class GameConfig
    {
        public RuleSet ruleSet;
        public int     playerCount;
        public int     bet;
    }

    public class Matchmaker : Singleton<Matchmaker>
    {
        #region Serializable

        [Header("Config")]
        public GameConfig config;

        [Serializable]
        public class MatchmakeResultEvent : UnityEvent<MatchmakeResultData>
        {
        }

        public MatchmakeResultEvent onMatchFound;
        public UnityEvent           onMatchNotFound;

        #endregion

        #region Properties
        
        [SerializeField]
        private bool lookingForMatches = false;
        public  bool LookingForMatches => lookingForMatches;

        private long _lastMatchmakeAttemptTime = 0;

        private MatchmakeResultData _currentMatchmakeResultData;

        public MatchmakeResultData CurrentMatchmakeResultData
        {
            get => _currentMatchmakeResultData;
            private set => _currentMatchmakeResultData = value;
        }

        #endregion

        public void OnEnable()
        {
            NetworkStream<MatchmakeResultData>.OnStreamHasItems += FoundMatch;
        }

        public void OnDisable()
        {
            NetworkStream<MatchmakeResultData>.OnStreamHasItems -= FoundMatch;
        }

        public void BeginMatchmake()
        {
            if (LookingForMatches) return;
            lookingForMatches = true;

            _lastMatchmakeAttemptTime = DateTime.Now.Ticks;

            var matchmakeRequestData = new MatchmakeRequestData {Bet = config.bet, PlayerCount = config.playerCount};

            ClientSend.MatchmakeRequest(ref matchmakeRequestData);
        }

        private void FoundMatch(IEnumerable<NetworkStream<MatchmakeResultData>.DataWrapper> results)
        {
            foreach (var data in results)
            {
                if (data.IsFresh(_lastMatchmakeAttemptTime))
                {
                    if(lookingForMatches)
                    {
                        //if not found a match, make sure to remove this data
                        if(!FoundMatch(data.Data)) data.MarkForRemoval();
                    }
                }
                else
                {
                    data.MarkForRemoval();
                }
            }
        }

        private bool FoundMatch(MatchmakeResultData data)
        {
            if (string.IsNullOrEmpty(data.Auth))
            {
                Debug.Log($"Failed to matchmake.");
                lookingForMatches = false;
                onMatchNotFound?.Invoke();
                return false;
            }
            else
            {
                Debug.Log($"Succeeded matchmake.\n {JsonImpl.ToJson(data)}");
                CurrentMatchmakeResultData = data;
                lookingForMatches         = false;
                onMatchFound?.Invoke(data);
                return true;
            }
        }
    }
}