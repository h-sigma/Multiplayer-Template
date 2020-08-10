using System;
using System.Collections;
using System.Collections.Generic;
using HarshCommon.Networking;
using HarshCommon.Patterns.Registry;
using HarshCommon.Utilities;
using Networking.Foundation;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Carrom
{
    public enum PlayerNumber
    {
        Player1 = 0,
        Player2 = 1,
        Player3 = 2,
        Player4 = 3,
        None = 4
    }

    public enum TokenColor
    {
        None  = -1,
        White = 0,
        Black = 1,
        Count = 2
    }

    public enum GameplayState
    {
        Setup,
        Stroke,
        Placing,
        CoverStroke
    }

    [DefaultExecutionOrder(-100)]
    public class CarromGameplay : CodeElements
    {
        #region Inspector

        [Header("Simulation Parameters")]
        public float velocityLimitSqrd;

        public BoardSimulation simulation;

        #region Events

        [Serializable]
        public class GameplayEvent : UnityEvent<CarromGameplay>
        {
        }

        public GameplayEvent onGameStart;

        [FormerlySerializedAs("onBeforePlayTurn")]
        public GameplayEvent onBeforeShootStriker;

        public GameplayEvent onAfterShootStriker;

        public GameplayEvent onStabilize;

        [Serializable]
        public class StateChangeEvent : UnityEvent<State>
        {
        }

        public StateChangeEvent onStateChange;

        public GameplayEvent onTurnChange;

        #endregion

        #endregion

        #region Classes

        [System.Serializable]
        public class PlayerProgress : IPacketSerializable
        {
            public void ReadFromPacket(Packet packet)
            {
                whitePocketed = packet.ReadInt();
                blackPocketed = packet.ReadInt();
                queenPocketed = packet.ReadBool();
                due           = packet.ReadInt();
            }

            public void WriteToPacket(Packet packet)
            {
                packet.Write(whitePocketed);
                packet.Write(blackPocketed);
                packet.Write(queenPocketed);
                packet.Write(due);
            }

            [SerializeField]
            private int whitePocketed = 0;

            [SerializeField]
            private int blackPocketed = 0;

            [SerializeField]
            private bool queenPocketed = false;

            [SerializeField]
            private int due = 0;

            [SerializeField]
            private bool firstPocketOfBoardDone = false;

            public int WhitePocketed
            {
                get => whitePocketed;
                set
                {
                    if (value > 0) firstPocketOfBoardDone = true;
                    whitePocketed = value;
                }
            }

            public int BlackPocketed
            {
                get => blackPocketed;
                set
                {
                    if (value > 0) firstPocketOfBoardDone = true;
                    blackPocketed = value;
                }
            }

            public bool QueenPocketed
            {
                get => queenPocketed;
                set => queenPocketed = value;
            }

            public int Due
            {
                get => due;
                set => due = value;
            }

            public bool HasDue => Due > 0;

            public bool FirstPocketOfBoardDone => firstPocketOfBoardDone;

            public void AddDue()
            {
                Due++;
            }
        }

        [System.Serializable]
        public class State
        {
            public int              turnId;
            public GameplayState    state;
            public PlayerNumber     awaitingTurn;
            public TokenColor       playerOneColor;
            public PlayerProgress[] progress;

            public State(int playerCount)
            {
                turnId         = 0;
                state          = GameplayState.Setup;
                awaitingTurn   = PlayerNumber.Player1;
                playerOneColor = TokenColor.White;
                progress       = new PlayerProgress[playerCount];
                for (int i = 0; i < progress.Length; i++)
                    progress[i] = new PlayerProgress();
            }

            public PlayerNumber ExpectedNextTurn(PlayerNumber finalPlayerNumberInRotation)
            {
                var result = awaitingTurn;
                if (awaitingTurn == finalPlayerNumberInRotation) //move back to player1
                {
                    result = PlayerNumber.Player1;
                }
                else //move to next player in chronological order
                {
                    result = (PlayerNumber) (((int) awaitingTurn) + 1);
                }

                return result;
            }

            public void NextTurn(bool nextTurn, PlayerNumber finalPlayerNumberInRotation)
            {
                //manage turn order
                if (!nextTurn)
                {
                    //do nothing
                    return;
                }

                awaitingTurn = ExpectedNextTurn(finalPlayerNumberInRotation);
            }
        }

        #endregion

        #region Fields

        private State _state;

        private Dictionary<GameTokenType, List<GameToken>> _pendingPocketResolutions = InitializeDict();

        private bool _isStableCache = false;

        [SerializeField]
        private bool _isBlockedByAnimation;

        [SerializeField]
        private int _lastTurnStarted;

        #endregion

        #region Pure Properties

        public State state => _state;

        public bool IsBlockedByAnimation
        {
            get => _isBlockedByAnimation;
            set => _isBlockedByAnimation = value;
        }

        public PlayerNumber AwaitingTurn => _state?.awaitingTurn ?? PlayerNumber.Player1;

        public bool IsStable
        {
            get
            {
                if (_isStableCache) return true;

                bool isStable = true;

                for (int i = 0, n = simulation.carrommen.Length; i < n; i++)
                {
                    var carromman = simulation.carrommen[i];
                    if (carromman != null && !carromman.IsScored && carromman.rigidbody.velocity.sqrMagnitude >=
                        velocityLimitSqrd)
                    {
                        isStable = false;
                        break; //early exit
                    }
                }

                if (simulation.striker.rigidbody.velocity.sqrMagnitude >= velocityLimitSqrd)
                {
                    isStable = false;
                }

                //set velocity to zero for good measure
                if (isStable)
                {
                    for (int i = 0, n = simulation.carrommen.Length; i < n; i++)
                    {
                        var carromman = simulation.carrommen[i];
                        if (carromman != null)
                        {
                            carromman.rigidbody.velocity = Vector3.zero;
                        }
                    }

                    simulation.striker.rigidbody.velocity = Vector3.zero;

                    onStabilize.Invoke(this);
                }

                _isStableCache = isStable;
                return isStable;
            }
        }

        #endregion

        #region Public

        public void BeginGame(Match match)
        {
            _state                = new State(match.PlayerCount);
            _isStableCache        = false;
            _isBlockedByAnimation = false;
            _lastTurnStarted      = 0;

            simulation.Generate();

            onGameStart.Invoke(this);
        }

        public IEnumerator TryDoTurn(TurnStartData turnStart, Action onCompleted, Action onFailed)
        {
            try
            {
                Assert.IsTrue(turnStart.PlayerNumber == _state.awaitingTurn,
                    "Tried to play a turn from player that wasn't being awaited.");
            }
            catch
            {
                onFailed?.Invoke();
                throw;
            }

            //do some animation
            onBeforeShootStriker.Invoke(this);
            
            _lastTurnStarted = turnStart.TurnId;

            yield return new WaitUntil(() => !IsBlockedByAnimation);

            DoTurn(turnStart.Baseline01, turnStart.AngleRad, turnStart.Force);

            onAfterShootStriker.Invoke(this);
                
            onCompleted?.Invoke();
        }

        public void Pocket(Pocket pocketedAt, GameToken token)
        {
            Assert.IsFalse(token.IsScored);
            _pendingPocketResolutions[token.RegisterableType].Add(token);
            token.TemporaryRemoveFromSimulation();
        }

        public bool CanStartTurn(int turnId)
        {
            return !IsBlockedByAnimation && turnId > _state.turnId && _lastTurnStarted < turnId;
        }

        public bool CanEndTurn(int turnId)
        {
            return !IsBlockedByAnimation && _lastTurnStarted == turnId && turnId > _state.turnId;
        }

        #endregion

        #region Rules/Pocket Handling (privates)

        private void DoTurn(float baselinePosition01, float angle, float force)
        {
            GameToken striker = default;

            if (SceneRegistry<GameTokenType, GameToken>.GetRandom(GameTokenType.Striker, gameObject.scene, out striker))
            {
                striker.ReturnToSimulation();
                DoStrikerShot(striker, baselinePosition01, angle, force);
            }
            else
            {
                Debug.LogError($"Striker not found in scene {this.gameObject.scene.name}");
                Debug.Break();
            }

            void ClearPendingPockets()
            {
                foreach (var pair in _pendingPocketResolutions)
                {
                    pair.Value.Clear();
                }
            }
        }

        private void DoStrikerShot(GameToken striker, float baselinePosition01, float angle, float force)
        {
            var dirx = Mathf.Cos(angle);
            var diry = Mathf.Sin(angle);

            var position = FindStrikerValidPosition(striker, baselinePosition01);

            striker.rigidbody.position = position;
            striker.rigidbody.AddForce(new Vector3(dirx, diry, 0) * force, ForceMode.Impulse);
        }

        private Vector3 FindStrikerValidPosition(GameToken striker, float baselinePosition01)
        {
            var baseline01     = baselinePosition01;
            var playerBaseline = simulation.GetBaseline(AwaitingTurn, Match.Instance.MatchData.PlayerNames.Length);

            var baselineSegments   = 20;
            var baselineCheckDelta = BoardMeasurements.BaseLineLength / baselineSegments;

            var testPosition = playerBaseline.GetPosition(baseline01);
            while (!IsValidPosition(striker, testPosition) && baseline01 <= 1.0f)
            {
                baseline01   += baselineCheckDelta;
                testPosition =  playerBaseline.GetPosition(baseline01);
            }

            baseline01 = baselinePosition01;
            while (!IsValidPosition(striker, testPosition) && baseline01 >= 0.0f)
            {
                baseline01   -= baselineCheckDelta;
                testPosition =  playerBaseline.GetPosition(baseline01);
            }

            return testPosition;
        }

        private bool IsValidPosition(GameToken token, Vector2 strikerPosition)
        {
            bool isValid        = true;
            var  tokenRadius    = token.transform.localScale.x;
            var  tokenRadiusSqr = tokenRadius * tokenRadius;
            for (int i = 0, n = simulation.carrommen.Length; i < n; i++)
            {
                var cm = simulation.carrommen[i];
                if (cm != null && cm != token)
                {
                    var     transform1         = cm.transform;
                    var     otherTokenRadius   = transform1.localScale.x;
                    Vector2 otherTokenPosition = transform1.position;
                    if ((otherTokenPosition - strikerPosition).sqrMagnitude <=
                        otherTokenRadius * otherTokenRadius + tokenRadiusSqr)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            return isValid;
        }

        #endregion

        private static Dictionary<GameTokenType, List<GameToken>> InitializeDict()
        {
            var result = new Dictionary<GameTokenType, List<GameToken>>();

            foreach (var value in Enum.GetValues(typeof(GameTokenType)))
            {
                result.Add((GameTokenType) value, new List<GameToken>());
            }

            return result;
        }

        private void Place(GameToken token)
        {
            //todo -- implement placing
            Debug.Log($"Placing {token.RegisterableType.ToString()} back into the board.");

            Vector2 center            = simulation.centerCircle.position;
            float   radius            = BoardMeasurements.CenterCircleOuterDiameter / 2.0f;
            Vector2 candidatePosition = center;

            int maxTries = 15;
            while (!IsValidPosition(token, candidatePosition))
            {
                candidatePosition = center + new Vector2(UnityEngine.Random.Range(-radius, +radius),
                    UnityEngine.Random.Range(-radius,                                      +radius));
                maxTries--;
                if (maxTries < 0)
                {
                    radius = BoardMeasurements.PlayingSide / 2.0f;
                    if (maxTries < 30)
                    {
                        break;
                    }
                }
            }

            token.rigidbody.position = candidatePosition;
        }

        #region Network Shenanigans

        public void LoadStateFromPendingTurn(TurnEndData pendingTurn)
        {
            foreach (var pair in _pendingPocketResolutions)
            {
                pair.Value.ForEach(token => token.ReturnToSimulation());
            }

            bool turnPlayerChanged = _state.awaitingTurn != pendingTurn.AwaitingTurn;

            _state.progress     = pendingTurn.Progresses;
            _state.state        = pendingTurn.CoverStroke ? GameplayState.CoverStroke : GameplayState.Stroke;
            _state.awaitingTurn = pendingTurn.AwaitingTurn;
            _state.turnId       = pendingTurn.turnId;

            if (SceneRegistry<GameTokenType, GameToken>.GetAllRegistered(GameTokenType.Black, this.gameObject.scene,
                out var blackCm))
            {
                for (int i = 0; i < pendingTurn.BlackCmPositions.Length && i < blackCm.Count; i++)
                {
                    var position = pendingTurn.BlackCmPositions[i];
                    blackCm[0].transform.position = position.IsNan() ? new Vector3(3000, 3000, 3000) : position;
                    blackCm[0].IsScored           = position.IsNan();
                }
            }
            else
            {
                Debug.Log($"No black tokens in scene.");
            }

            if (SceneRegistry<GameTokenType, GameToken>.GetAllRegistered(GameTokenType.White, this.gameObject.scene,
                out var whiteCm))
            {
                for (int i = 0; i < pendingTurn.WhiteCmPositions.Length && i < whiteCm.Count; i++)
                {
                    var position = pendingTurn.WhiteCmPositions[i];
                    whiteCm[0].transform.position = position.IsNan() ? new Vector3(3000, 3000, 3000) : position;
                    whiteCm[0].IsScored           = position.IsNan();
                }
            }
            else
            {
                Debug.Log($"No white tokens in scene.");
            }

            if (SceneRegistry<GameTokenType, GameToken>.GetAllRegistered(GameTokenType.Queen, this.gameObject.scene,
                out var queen))
            {
                if (queen.Count == 0 || queen.Count > 1)
                {
                    Debug.Log($"Illegal number of queen in scene.");
                }
                else
                {
                    var position = pendingTurn.QueenCmPosition;
                    queen[0].transform.position = position.IsNan() ? new Vector3(3000, 3000, 3000) : position;
                    queen[0].IsScored           = position.IsNan();
                }
            }
            else
            {
                Debug.Log($"No queens in scene.");
            }

            onStateChange.Invoke(state);

            if (turnPlayerChanged) onTurnChange.Invoke(this);
        }

        #endregion

        public void Awake()
        {
            onStateChange.AddListener((stte) => { _isStableCache = false; });
            //todo -- ??
        }
    }
}