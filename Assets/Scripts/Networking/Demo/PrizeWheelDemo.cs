using System;
using System.Text;
using Networking.PlayFabCustom;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Networking.Demo
{
    public class PrizeWheelDemo : MonoBehaviour
    {
        public Button          spin            = default;
        public TextMeshProUGUI spinTicketBalance     = default;
        public TextMeshProUGUI timeUntilNextFreeSpin = default;
        public TextMeshProUGUI itemCount             = default;

        public void Start()
        {
            if (!PlayFabTitleConfig.instance.IsLoggedIn)
            {
                PlayFabAuthService.OnLoginSuccess += GetInventory;
                PlayFabAuthService.Instance.Authenticate(Authtypes.Silent);
            }
            else
            {
                GetInventory();
            }
        }

        public void OnDestroy()
        {
            PlayFabAuthService.OnLoginSuccess -= GetInventory;
        }
        
        void SetUI(int tickets, int items, string nextFree)
        {
            this.spinTicketBalance.text     = string.Format("x{0}", tickets);
            this.itemCount.text             = string.Format("{0}",  items);
            this.timeUntilNextFreeSpin.text = string.Format("{0}",  nextFree);
        }
	
        void UnlockUI()
        {
            this.spin.interactable = true;
            this.itemCount.gameObject.SetActive(true);
            this.timeUntilNextFreeSpin.gameObject.SetActive(true);
            this.spinTicketBalance.gameObject.SetActive(true);
        }
	
        void LockUI()
        {
            this.spin.interactable = false;
            this.itemCount.gameObject.SetActive(false);
            this.timeUntilNextFreeSpin.gameObject.SetActive(false);
            this.spinTicketBalance.gameObject.SetActive(false);
        }

        private void GetInventory(LoginResult result = null)
        {
            LockUI();
            var request = new GetUserInventoryRequest();
            PlayFabClientAPI.GetUserInventory(request, GetInventoryCallback, OnApiCallError);
        }

        private void GetInventoryCallback(GetUserInventoryResult result)
        {
            Debug.Log($"Inventory retrieved. You have {result.Inventory.Count} items.");

            int stBalance;
            result.VirtualCurrency.TryGetValue("ST", out stBalance);

            string nextTicketDisplay = "Capped";
            if (result.VirtualCurrencyRechargeTimes.TryGetValue("ST", out var rechargeDetails))
            {
                if (stBalance < rechargeDetails.RechargeMax)
                {
                    var nextTicketTime = new DateTime();
                    nextTicketTime = DateTime.Now.AddSeconds(rechargeDetails.SecondsToRecharge);
                    Debug.Log($"You have {stBalance} Spin Tickets.");
                    Debug.Log($"Your next free ticket will arrive at {nextTicketTime}");
                    nextTicketDisplay = nextTicketTime.ToString();
                }
                else
                {
                    Debug.Log($"Tickets only go up to a maximum of {rechargeDetails.RechargeMax}, and you currently have {stBalance}");
                }
            }
            
            SetUI(stBalance, result.Inventory.Count, nextTicketDisplay);
            UnlockUI();
        }

        public void TryToSpin()
        {
            Debug.Log("Attempting to spin...");
            var request = new PurchaseItemRequest() {ItemId = "PrizeWheel1", VirtualCurrency = "ST", Price = 1};
            PlayFabClientAPI.PurchaseItem(request, TryToSpinCallback, OnApiCallError);
        }

        private void TryToSpinCallback(PurchaseItemResult result)
        {
            Debug.Log("Ticket accepted! \nSpinning...");
            foreach (var item in result.Items)
            {
                Debug.Log($"Item: {item.DisplayName}");
            }
            
            GetInventory();
        }

        private void OnApiCallError(PlayFabError err)
        {
            string        http    = $"HTTP:{err.HttpCode}";
            string        message = $"ERROR:{err.Error} -- {err.ErrorMessage}";
            StringBuilder details = new StringBuilder();

            if (err.ErrorDetails != null)
            {
                foreach (var detail in err.ErrorDetails)
                {
                    details.Append(string.Format("{0} \n", detail.ToString()));
                }
            }

            Debug.LogError($"{http}\n {message}\n {details.ToString()}\n");
        }
    }
}