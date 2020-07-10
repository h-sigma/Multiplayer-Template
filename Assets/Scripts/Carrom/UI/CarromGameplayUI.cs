using TMPro;
using UnityEngine;

namespace Carrom.UI
{
    public class CarromGameplayUI : MonoBehaviour
    {
        [Header("Scene References")]
        public CarromGameplay gameplay;

        public TextMeshProUGUI turnIndicator;

        public void OnEnable()
        {
            UpdateUI();
            gameplay.AfterTurnPlayed += UpdateUI;
        }

        public void OnDisable()
        {
            gameplay.AfterTurnPlayed -= UpdateUI;
        }

        private void UpdateUI()
        {
            turnIndicator.text = gameplay.AwaitingTurn.ToString();
        }
    }
}
