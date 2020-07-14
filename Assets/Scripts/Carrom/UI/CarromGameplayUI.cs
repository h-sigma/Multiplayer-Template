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
            gameplay.onStateChange.AddListener(UpdateUI);
        }

        public void OnDisable()
        {
            gameplay.onStateChange.RemoveListener(UpdateUI);
        }

        private void UpdateUI()
        {
            turnIndicator.text = gameplay.AwaitingTurn.ToString();
        }
    }
}
