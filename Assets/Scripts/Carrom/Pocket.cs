using System.Linq;
using HarshCommon.Patterns.Registry;
using UnityEngine;

namespace Carrom
{
    public class Pocket : MonoBehaviour
    {
        public CarromGameplay gameplay;

        private void OnEnable()
        {
            var gameplayGo = this.GetAllRegisteredInScene<CodeElements, CodeElementsType>(CodeElementsType.Gameplay).FirstOrDefault();
            if (gameplayGo != null)
            {
                gameplay = gameplayGo.GetComponentInChildren<CarromGameplay>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var token = other.gameObject.GetComponent<GameToken>();
            HandlePocketedToken(token);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var token = other.gameObject.GetComponent<GameToken>();
            HandlePocketedToken(token);
        }

        private void HandlePocketedToken(GameToken token)
        {
            if (token == null) return;
            gameplay.Pocket(this, token);
        }
    }
}
