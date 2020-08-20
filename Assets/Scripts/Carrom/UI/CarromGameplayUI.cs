using System.Linq;
using Networking.Foundation;
using TMPro;
using UnityEngine;

namespace Carrom.UI
{
    public class CarromGameplayUI : MonoBehaviour
    {
        /*
         * Visual Elements:
         * 1. Position Slider
         * 2. Striker
         * 3. Striker AIM UI
         * 4. Player UI
         * 5. Turn Indicator (Which Player Has Turn)
         * 6. Cover Stroke Indicator
         * 7. Board Orientation
         */
        
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

            for (int i = matchData.PlayerCount; i < players.Length; i++)
            {
                players[i].SetUnused();
            }

            turnIndicator.text = "---";
            coverShotIndicator.gameObject.SetActive(false);
        }

        public void UpdateUI(CarromGameplay.State state)
        {
            if (state == null) return;
            
            turnIndicator.text = players.FirstOrDefault(p => p.number == state.awaitingTurn)?.playerName.text ?? "---";
            coverShotIndicator.gameObject.SetActive(state.state == GameplayState.CoverStroke);
            for (int i = 0; i < players.Length; i++)
            {
                players[i].UpdateUI(state);
            }
        }
    }
}