using System;

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

        private bool lookingForMatches = false;

        public bool LookingForMatches => lookingForMatches;

        private Action<MatchmakeResultData> _onSuccess = default;
        private Action                      _onFailure = default;

        public void TryMatchmaking(Action<MatchmakeResultData> onSuccess, Action onFailure)
        {
            var matchmakeRequestData = new MatchmakeRequestData();
            matchmakeRequestData.Bet = config.bet;
            matchmakeRequestData.twoPlayer = twoPlayer;

            ClientSend.MatchmakeRequest(ref matchmakeRequestData);
        }

        public void FoundMatch(MatchmakeResultData data)
        {
            if (string.IsNullOrEmpty(data.Auth))
            {
                _onFailure?.Invoke();
            }
            else
            {
                _onSuccess?.Invoke(data);
            }
        }
    }
}