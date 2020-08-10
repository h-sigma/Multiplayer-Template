using Networking.Foundation;
using TMPro;
using UGUIFundamentals;
using UnityEngine;

namespace Carrom.UI
{
    public class MatchPlayerUI : MonoBehaviour
    {
        public TextMeshProUGUI playerName;
        public SimpleAvatar    avatar;

        public TextMeshProUGUI pockets;
        public TextMeshProUGUI dues;
        public RectTransform   hasQueenIndicator;

        public PlayerNumber number = PlayerNumber.None;

        public                  Animator onOwnTurnAnimate;
        private static readonly int      s_Animate = Animator.StringToHash("turn");

        public bool IsUsed { get; set; } = false;

        public void Awake()
        {
            if (onOwnTurnAnimate == null) GetComponent<Animator>();
        }

        public void Setup(int index, ref MatchData matchData)
        {
            avatar.url = matchData.AvatarURIs[index];
            avatar.LoadAvatar();

            playerName.text = matchData.PlayerNames[index];

            number = (PlayerNumber) index;

            IsUsed = true;
        }

        public void SetUnused()
        {
            gameObject.SetActive(false);
        }

        public void SetUsed()
        {
            gameObject.SetActive(true);
        }

        public void UpdateUI(CarromGameplay.State state)
        {
            if (state == null || state.progress.Length <= (int) number)
            {
                SetUnused();
                return;
            }

            SetUsed();

            var progress = state.progress[(int) number];

            pockets.text = progress.HasDue
                ? progress.Due.ToString()
                : (progress.WhitePocketed + progress.BlackPocketed).ToString();
            hasQueenIndicator.gameObject.SetActive(progress.QueenPocketed);

            onOwnTurnAnimate.SetBool(s_Animate, state.awaitingTurn == number);
        }
    }
}