using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Networking.Foundation;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

namespace Carrom
{
    public enum PlayerNumber
    {
        Player1 = 0,
        Player2 = 1,
        Player3 = 2,
        Player4 = 3
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

        #endregion

        #region Fields/Properties/Events

        [System.Serializable]
        public class PlayerProgress : IPacketSerializable
        {
            public void ReadFromPacket(Packet packet)
            {
                whitePocketed = packet.ReadInt();
                blackPocketed = packet.ReadInt();
                queenPocketed = packet.ReadBool();
                due = packet.ReadInt();
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

        private State                                      _state;
        
        private Dictionary<GameTokenType, List<GameToken>> _pendingPocketResolutions = InitializeDict();

        public PlayerNumber AwaitingTurn => _state.awaitingTurn;

        #endregion

        public void TryDoTurn(PlayerNumber playerNumber, float baselinePosition01, float angle, float force)
        {
            if (playerNumber != _state.awaitingTurn) return;
            DoTurn(baselinePosition01, angle, force);
        }

        public void Pocket(Pocket pocketedAt, GameToken token)
        {
            Assert.IsFalse(token.IsScored);
            _pendingPocketResolutions[token.RegisterableType].Add(token);
        }

        private void DoTurn(float baselinePosition01, float angle, float force)
        {
            //notify listeners
            BeforeTurnPlayed?.Invoke();

            //simulate board
            simulation.SetManual(true);

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

            /*var min = MinimumSimulationSteps;
            while (!ExitCondition() || min > 0)
            {
                min--;
                simulation.Simulate(simulationStep, simulationStep);
                yield return new WaitForFixedUpdate();
            }
            Debug.Break();*/

            simulation.Simulate(simulationStep, ExitCondition, MinimumSimulationSteps, MaximumSimulationSteps);
            if (striker != null) striker.TemporaryRemoveFromSimulation();
            simulation.SetManual(false);

            //manage pockets
            bool nextTurn = true;
            if (_pendingPocketResolutions.Count > 0)
            {
                //todo -- Simply do an if-else with the rules
                var blackList   = _pendingPocketResolutions[GameTokenType.Black];
                var whiteList   = _pendingPocketResolutions[GameTokenType.White];
                var queenList   = _pendingPocketResolutions[GameTokenType.Queen];
                var strikerList = _pendingPocketResolutions[GameTokenType.Striker];

                nextTurn = ResolveProgress(whiteList, blackList, queenList, strikerList);

                ClearPendingPockets();
            }

            _state.NextTurn(nextTurn, finalPlayerNumberInRotation);

            //notify listeners
            AfterTurnPlayed?.Invoke();

            //end of method code
            //follows: local methods

            void ClearPendingPockets()
            {
                foreach (var pair in _pendingPocketResolutions)
                {
                    pair.Value.Clear();
                }
            }

            // Exit Condition: Velocity of all carrommen + striker is below a certain range
            bool ExitCondition()
            {
                bool exit = true;

                for (int i = 0, n = simulation.carrommen.Length; i < n; i++)
                {
                    var carromman = simulation.carrommen[i];
                    if (carromman != null && !carromman.IsScored && carromman.rigidbody.velocity.sqrMagnitude >=
                        velocityLimitSqrd)
                    {
                        exit = false;
                        break; //early exit
                    }
                }

                if (simulation.striker.rigidbody.velocity.sqrMagnitude >= velocityLimitSqrd)
                {
                    exit = false;
                }

                //set velocity to zero for good measure
                if (exit)
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
                }

                return exit;
            }
        }

        private void DoStrikerShot(GameToken striker, float baselinePosition01, float angle, float force)
        {
            var dirx = Mathf.Cos(angle);
            var diry = Mathf.Sin(angle);

            var position = FindStrikerValidPosition(striker, baselinePosition01);
            
            Debug.Log($"Position changed from {striker.rigidbody.position.ToString("F4")} to {position.ToString("F4")}.");

            striker.rigidbody.position = position;
            striker.rigidbody.AddForce(new Vector3(dirx, diry, 0) * force, ForceMode.Impulse);
        }

        private Vector3 FindStrikerValidPosition(GameToken striker, float baselinePosition01)
        {
            var baseline01     = baselinePosition01;
            var playerBaseline = simulation.GetBaseline(AwaitingTurn, _config.PlayerCount);

            var segments = 20;
            var baselineCheckDelta = BoardMeasurements.BaseLineLength / segments;

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

        private static Dictionary<GameTokenType, List<GameToken>> InitializeDict()
        {
            var result = new Dictionary<GameTokenType, List<GameToken>>();

            foreach (var value in Enum.GetValues(typeof(GameTokenType)))
            {
                result.Add((GameTokenType) value, new List<GameToken>());
            }

            return result;
        }

        private bool ResolveProgress(List<GameToken> whiteList, List<GameToken> blackList,
            List<GameToken>                          queenList, List<GameToken> strikerList)
        {
            #region Condition Variables

            var didPocketStriker = strikerList.Count > 0;
            var didPocketQueen   = queenList.Count   > 0;

            var playerColor   = GetPlayerColor();
            var opponentColor = GetOtherColor(playerColor);

            var ownCmPocketed = playerColor == TokenColor.White ? whiteList : blackList;
            var oppCmPocketed = playerColor == TokenColor.White ? blackList : whiteList;

            var ownCmCount = ownCmPocketed.Count;
            var oppCmCount = oppCmPocketed.Count;

            var playerProgress = _state.progress[(int) _state.awaitingTurn];
            var oppProgress    = _state.progress[(int) _state.ExpectedNextTurn(finalPlayerNumberInRotation)];

            var ownRemainingOnBoard = simulation.carrommen.Count(token =>
            {
                return token != null && !token.IsScored &&
                       (playerColor == TokenColor.White
                           ? (token.RegisterableType == GameTokenType.White)
                           : (token.RegisterableType == GameTokenType.Black));
            });
            var oppRemainingOnBoard = simulation.carrommen.Count(token =>
            {
                return token != null && !token.IsScored &&
                       (playerColor == TokenColor.White
                           ? (token.RegisterableType == GameTokenType.Black)
                           : (token.RegisterableType == GameTokenType.White));
            });

            bool queenNotScored = simulation.carrommen.Any(token =>
            {
                return token                  != null && !token.IsScored &&
                       token.RegisterableType == GameTokenType.Queen;
            });

            bool queenOnBoard = queenNotScored && RemovedQueen == null;

            var lastCmOfPlayerPocketed = ownRemainingOnBoard == ownCmCount && ownCmCount > 0;
            var lastCmOfOppPocketed    = oppRemainingOnBoard == oppCmCount && oppCmCount > 0;

            #endregion

            //result variables
            var nextTurn = true;

            #region Rule 1 : Own C/m Pocketed

            //rule1: Pocketing own C/m always gives player next turn. If pocketed striker also, own pockets are placed back, plus due.
            if (ownCmCount > 0)
            {
                nextTurn = false;
                if (didPocketStriker)
                {
                    PlaceTokens(ownCmPocketed);
                }
                else
                {
                    AwardTokens(ownCmPocketed, playerProgress);
                }
            }

            #endregion

            #region Rule 2 : Opponent C/m Pocketed

            //rule2: Pocketing opponent C/m always awards to opponent.
            if (oppCmCount > 0)
            {
                AwardTokens(oppCmPocketed, oppProgress);
            }

            #endregion

            #region Rule 3 : Striker Pocketed

            //rule3: Pocketing striker yields a penalty.
            if (didPocketStriker)
            {
                AddDueFromStriker();
                PlaceTokens(strikerList);
            }

            #endregion

            #region Rule 4 : Cover Strokes

            //rule4: When trying to cover the queen,
            if (_state.state == GameplayState.CoverStroke)
            {
                if (ownCmCount > 0)
                {
                    if (didPocketStriker)
                    {
                        //a) if player pockets own C/m with striker, he may try to cover queen again
                        _state.state = GameplayState.CoverStroke; //important: player gets to continue turn
                    }
                    else
                    {
                        //b) if player pockets own C/m, he is awarded the queen
                        AwardRemovedQueen(playerProgress);
                        _state.state = GameplayState.Stroke;
                    }
                }
                //c) if player pockets no C/m while covering, the queen is brought back for placing
                else if (ownCmCount == 0)
                {
                    _state.state = GameplayState.Stroke;
                    PlaceRemovedQueenBack();
                }
            }

            #endregion

            #region Rule 5 : Pocketing Queen

            //rule 5: In normal stroke, upon pocketing the queen
            if (didPocketQueen)
            {
                if (ownRemainingOnBoard == 9
                ) //calculated before awarding any C/m, so it's okay to depend on this number
                {
                    //a) if all 9 of player's C/m are on the board,
                    if (ownCmCount == 1)
                    {
                        //a.1) if pocketed queen together with one C/m, queen has to be covered
                        RemoveQueenForCovering();
                        _state.state = GameplayState.CoverStroke;
                    }
                    else if (ownCmCount > 1)
                    {
                        //a.2 if pocketed queen together with two or more C/m, queen is considered covered
                        AwardTokens(queenList, playerProgress);
                        _state.state = GameplayState.Stroke;
                    }
                    else if (ownCmCount == 0)
                    {
                        //a.3 if pocketed only queen, lose turn and place queen
                        PlaceTokens(queenList);
                        _state.state = GameplayState.Stroke; // for completion
                    }
                }
                else if (playerProgress.HasDue) //dynamically calculated
                {
                    //b) if player has due, queen is placed back, and player loses turn
                    nextTurn = false;
                    PlaceTokens(queenList);
                    _state.state = GameplayState.Stroke;
                }
                else if (ownCmCount > 0)
                {
                    //c) if player pockets one or more of his C/m with queen, it is considered covered
                    AwardTokens(queenList, playerProgress);
                    _state.state = GameplayState.Stroke;
                }
                else if (ownCmCount == 0)
                {
                    //d) if player pockets only queen, they get next turn and can try to cover
                    _state.state = GameplayState.CoverStroke;
                    nextTurn     = false;
                    RemoveQueenForCovering();
                }
            }

            #endregion

            #region Rule 6 : Win/Lose Conditions

            //Rule 6 : Win or Lose
            if (_state.state == GameplayState.CoverStroke)
            {
                if (lastCmOfOppPocketed && lastCmOfPlayerPocketed)
                {
                    //a) If player pockets last of his own C/m along with opponent's last C/m, he wins the board
                    PlayerWins();
                }
                else if (lastCmOfOppPocketed)
                {
                    //b) If player pockets last of opp's C/m, opp wins the board
                    OpponentWins();
                }
                else if (lastCmOfPlayerPocketed)
                {
                    //c) If player pockets last of his C/m along with queen, he wins the board
                    PlayerWins();
                }
            }
            else if (queenOnBoard)
            {
                if (lastCmOfOppPocketed || lastCmOfPlayerPocketed)
                {
                    OpponentWins();
                }
            }
            else if (playerProgress.QueenPocketed)
            {
                if (didPocketStriker && lastCmOfOppPocketed && lastCmOfPlayerPocketed)
                {
                    OpponentWins();
                }
            }

            #endregion

            //todo -- settle Due

            return nextTurn;

            #region Local Methods

            TokenColor GetPlayerColor()
            {
                var player = _state.awaitingTurn;
                if (player == PlayerNumber.Player1 || player == PlayerNumber.Player3)
                    return _state.playerOneColor;
                else
                {
                    var playerTwoColor = (TokenColor) (((int) _state.playerOneColor + 1) % (int) TokenColor.Count);
                    return playerTwoColor;
                }
            }

            void AwardTokens(List<GameToken> tokens, PlayerProgress awardTo)
            {
                for (int i = 0, n = tokens.Count; i < n; i++)
                {
                    Award(tokens[i], awardTo);
                }
            }

            void PlaceTokens(List<GameToken> tokens)
            {
                for (int i = 0, n = tokens.Count; i < n; i++)
                {
                    Place(tokens[i]);
                }
            }

            void AddDueFromStriker()
            {
                playerProgress.AddDue();
            }

            void AwardRemovedQueen(PlayerProgress awardTo)
            {
                Assert.IsNotNull(RemovedQueen, "RemovedQueen != null when awarding covered queen");
                Award(RemovedQueen, awardTo);
                RemovedQueen = null;
            }

            void PlaceRemovedQueenBack()
            {
                Assert.IsNotNull(RemovedQueen, "RemovedQueen != null when placing back");
                Place(RemovedQueen);
                RemovedQueen = null;
            }

            void RemoveQueenForCovering()
            {
                var queen = queenList.FirstOrDefault();
                Assert.IsNotNull(queen, "queen != null when removing for covering");
                RemovedQueen = queen;
            }

            void PlayerWins()
            {
                Debug.Log($"The current player {_state.awaitingTurn} has won.");
            }

            void OpponentWins()
            {
                Debug.Log($"The other player {_state.ExpectedNextTurn(finalPlayerNumberInRotation)} has won.");
            }

            #endregion
        }

        private static TokenColor GetOtherColor(TokenColor color)
        {
            return color == TokenColor.Black ? TokenColor.White : TokenColor.Black;
        }

        private GameToken _removedQueen = null;
        private Match _match;

        public GameToken RemovedQueen
        {
            get => _removedQueen;
            set
            {
                if (value == null)
                {
                    if (_removedQueen != null)
                    {
                        _removedQueen.ReturnToSimulation();
                    }
                }
                else
                {
                    value.TemporaryRemoveFromSimulation();
                }

                _removedQueen = value;
            }
        }

        private void Award(GameToken token, PlayerProgress awardedPlayer)
        {
            Debug.Log($"{token.RegisterableType.ToString()} token awarded to {awardedPlayer.ToString()}.");

            switch (token.RegisterableType)
            {
                case GameTokenType.Queen:
                    awardedPlayer.QueenPocketed = true;
                    break;
                case GameTokenType.Black:
                    awardedPlayer.BlackPocketed++;
                    break;
                case GameTokenType.White:
                    awardedPlayer.WhitePocketed++;
                    break;
                case GameTokenType.Striker:
                    throw new ArgumentException("Striker awarded!");
                    break;
            }

            token.IsScored = true;
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

        public void BeginGame(Match match)
        {
            _match = match;
            
            finalPlayerNumberInRotation = (PlayerNumber) (config.PlayerCount - 1);

            _state = new State(config.PlayerCount);

            simulation.Generate();
        }
    }
}