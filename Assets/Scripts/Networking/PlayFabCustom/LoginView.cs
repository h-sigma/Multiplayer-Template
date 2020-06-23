using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Networking.PlayFabCustom
{
    public class LoginView : MonoBehaviour
    {
        public LoginController controller;
        public SceneTransition transitionOutOfLogin;

        #region Default UI Items

        [Header("Default UI Items")]
        [SerializeField]
        private RectTransform loadingPanel = default;

        [Header("EmailPass Login")]
        [SerializeField]
        private TMP_InputField emailInputField = default;

        [SerializeField]
        private TMP_InputField passInputField = default;

        [SerializeField]
        private Toggle rememberMeEmailPassToggle = default;

        [Header("Account")]
        [SerializeField]
        private TMP_InputField accountEmailInputField = default;

        [SerializeField]
        private TMP_InputField accountPassInputField = default;

        [SerializeField]
        private Toggle rememberMeAccountToggle = default;

        [SerializeField]
        private RectTransform errorMessage = default;

        #endregion

        public void Awake()
        {
            gameObject.GetComponentInChildrenIfNull(ref controller);
            controller.onStateChanged.AddListener(SyncViewToControllerState);
        }

        private void SyncViewToControllerState(LoginController.State prev, LoginController.State next, object data)
        {
            errorMessage.gameObject.SetActive(next == LoginController.State.Error);
            loadingPanel.gameObject.SetActive(next == LoginController.State.TryLogin);
            if (data is PlayFabError error)
            {
                var textbox = errorMessage.GetComponentInChildren<TextMeshProUGUI>();
                if (textbox != null)
                    textbox.text = error.GenerateErrorReport();
            }
            else if (next == LoginController.State.Error)
            {
                var textbox = errorMessage.GetComponentInChildren<TextMeshProUGUI>();
                if (textbox != null)
                    textbox.text = "Unknown error occured.";
            }
            
            if (data is LoginResult result)
            {
                transitionOutOfLogin.TryTransition(true);
            }
        }

        public void AttemptEmailLogin()
        {
            if (emailInputField == null || passInputField == null || rememberMeEmailPassToggle == null)
                return;
            controller.TryEmailPasswordLogin(emailInputField.text, passInputField.text, rememberMeEmailPassToggle.isOn);
        }

        public void AttemptAccountRegister()
        {
            if (accountEmailInputField == null || accountPassInputField == null || rememberMeAccountToggle == null)
                return;
            controller.RegisterAccount(accountEmailInputField.text, accountPassInputField.text,
                rememberMeAccountToggle.isOn);
        }
    }
}