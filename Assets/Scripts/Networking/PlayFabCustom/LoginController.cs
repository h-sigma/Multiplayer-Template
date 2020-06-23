using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

namespace Networking.PlayFabCustom
{
    public class LoginController : MonoBehaviour
    {
        public enum State
        {
            Default,
            Error,
            TryLogin,
            Success
        }

        [System.Serializable]
        public class StateChangedEvent : UnityEvent<State, State, object>
        {
        }

        #region UI Callbacks // Awake

        [Header("Events")]
        public StateChangedEvent onStateChanged;

        private State _currentState;

        #endregion

        public void Start()
        {
            if (PlayFabTitleConfig.instance.AllowSilentSignup)
            {
                SignalState(State.Default);
                SubscribeToAuthService();
                PlayFabAuthService.Instance.Authenticate(Authtypes.Silent);
                SignalState(State.TryLogin);
            }
        }

        public void TryEmailPasswordLogin(string email, string pass,
            bool                                 rememberMe = false)
        {
            SignalState(State.Default);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                AuthFailure(null);
                return;
            }

            PlayFabAuthService.Instance.Email    = email;
            PlayFabAuthService.Instance.Password = pass;

            PlayFabAuthService.Instance.RememberMe = rememberMe;

            SubscribeToAuthService();
            PlayFabAuthService.Instance.Authenticate(Authtypes.EmailAndPassword);

            SignalState(State.TryLogin);
        }

        public void RegisterAccount(string email, string pass, bool rememberMe = false)
        {
            SignalState(State.Default);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass))
            {
                AuthFailure(null);
                return;
            }

            PlayFabAuthService.Instance.Email    = email;
            PlayFabAuthService.Instance.Password = pass;

            PlayFabAuthService.Instance.RememberMe = rememberMe;

            SubscribeToAuthService();
            //if (PlayFabTitleConfig.instance.AllowSilentSignup)

            //link existing 
            PlayFabAuthService.Instance.Authenticate(Authtypes.RegisterPlayFabAccount);

            SignalState(State.TryLogin);
        }

        #region Auth Event Handlers

        private bool subbed = false;

        private void SubscribeToAuthService()
        {
            if (subbed) return;
            PlayFabAuthService.OnLoginSuccess += AuthSuccess;
            PlayFabAuthService.OnPlayFabError += AuthFailure;
            subbed                            =  true;
        }

        private void UnsubscribeFromAuthService()
        {
            if (!subbed) return;
            PlayFabAuthService.OnLoginSuccess -= AuthSuccess;
            PlayFabAuthService.OnPlayFabError -= AuthFailure;
            subbed                            =  false;
        }

        private void AuthSuccess(LoginResult success)
        {
            UnsubscribeFromAuthService();

            SignalState(State.Success, success);
            PlayFabTitleConfig.instance.IsLoggedIn = true;
        }

        private void AuthFailure(PlayFabError error)
        {
            UnsubscribeFromAuthService();

            SignalState(State.Error, error);
            PlayFabTitleConfig.instance.IsLoggedIn = false;
#if LOG_NETWORK
            if(error != null)
                Debug.Log("Auth failed: " + error.GenerateErrorReport(), this);
#endif
        }

        private void SignalState(State state, object data = null)
        {
            var temp = _currentState;
            _currentState = state;
            onStateChanged?.Invoke(temp, state, data);
        }

        #endregion
    }
}