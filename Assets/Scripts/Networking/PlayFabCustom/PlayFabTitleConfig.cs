using System;
using UnityEngine;
using Utility;

namespace Networking.PlayFabCustom
{
    [CreateAssetMenu(menuName = "Scriptables/PlayFab Title Config")]
    public class PlayFabTitleConfig : ScriptableSingleton<PlayFabTitleConfig>
    {
        public static readonly string SilentAuthAllowed = "Allow Silent Auth";
        
        [NonSerialized]
        public bool IsLoggedIn = false;

        [ThroughProperty(nameof(AllowSilentSignup)), SerializeField]
        private bool allowSilentSignupPropertyProxy;
        public bool AllowSilentSignup
        {
            get => PlayerPrefs.GetInt(SilentAuthAllowed, 0) == 1;
            set => PlayerPrefs.SetInt(SilentAuthAllowed, value ? 1 : 0);
        }

        [Tooltip("Types of authentication allowed in build.")]
        public Authtypes[] allowedAuthTypes;
    }
}