using Networking.Foundation;
using TMPro;
using UnityEngine;

namespace Carrom.UI
{
    public class CarromGameplayUI : MonoBehaviour
    {
        public TextMeshProUGUI turnIndicator;
        public MatchPlayerUI[] players = new MatchPlayerUI[4];
        public RectTransform   coverShotIndicator;

        public void SetupPlayers(MatchData matchData)
        {
            var ownPlayer = (int) matchData.YourPlayerNumber;
            players[0].Setup(ownPlayer, ref matchData);

            var currentPlayer = PlayerNumber.Player1;

            for (int i = 1; i < matchData.PlayerCount; i++)
            {
                if ((int) currentPlayer == ownPlayer) currentPlayer++; //skip over own player
                players[i].Setup((int) currentPlayer, ref matchData);
                currentPlayer++;
            }

            turnIndicator.text = matchData.PlayerNames[0];
            coverShotIndicator.gameObject.SetActive(false);
        }

        public void UpdateUI(CarromGameplay.State state)
        {
            if (state == null) return;
            
            turnIndicator.text = players[(int) state.awaitingTurn].playerName.text;
            coverShotIndicator.gameObject.SetActive(state.state == GameplayState.CoverStroke);
            for (int i = 0; i < players.Length; i++)
            {
                players[i].UpdateUI(state);
            }
        }
    }
}