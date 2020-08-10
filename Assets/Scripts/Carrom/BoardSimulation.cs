using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carrom
{
    public class BoardSimulation : PhysicsSimulation
    {
        #region Inspector
        
        [Header("Scene References")]
        public GameObject board;
        public Transform  centerCircle;
        public Striker striker;

        public List<Line> baselines;

        [Header("Prefabs")]
        public GameToken whiteManPrefab;
        public GameToken blackManPrefab;
        public GameToken queenPrefab;

        #endregion

        [NonSerialized]
        public GameToken[] carrommen = new GameToken[19]; //queen + 9 white + 9 black

        public Vector2[] FinalResult;

        public override void Awake()
        {
            base.Awake();
            SetManual(false);
        }
        
        public override void Generate()
        {
            DestroyExisting();
            var queen  = Instantiate(queenPrefab, centerCircle, false);
            carrommen[0] = queen;
            
            var radius = BoardMeasurements.CarrommenDiameter / 2.0f;
            var hypot  = 2                                   * radius;
            var adj30  = hypot                               * Mathf.Cos(30 * Mathf.Deg2Rad);
            var opp30  = hypot                               * Mathf.Sin(30 * Mathf.Deg2Rad);
            
            //inner hexagon
            var inner = new[]
            {
                new Vector2(0,      hypot),  // 0   deg
                new Vector2(adj30,  opp30),  // 60  deg
                new Vector2(adj30,  -opp30), // 120 deg
                new Vector2(0,      -hypot), // 180 deg
                new Vector2(-adj30, -opp30), // 240 deg
                new Vector2(-adj30, opp30),  // 300 deg
            };

            Vector2[] outer = new Vector2[6];
            for (int i = 0, n = inner.Length; i < n; i++)
            {
                outer[i] = inner[i] * 2;
            }

            var innerResult = PlaceAlongPath(inner, radius);
            var outerResult = PlaceAlongPath(outer, radius);

            FinalResult = new Vector2[innerResult.Length + outerResult.Length];
            innerResult.CopyTo(FinalResult, 0);
            outerResult.CopyTo(FinalResult, innerResult.Length);
            
            int index = 0;
            foreach (var r in FinalResult)
            {
                index++;
                carrommen[index] = Instantiate(index % 2 == 0 ? whiteManPrefab : blackManPrefab, centerCircle);
                carrommen[index].transform.localPosition = r;
            }
        }

        private void DestroyExisting()
        {
            for (int i = 0; i < carrommen.Length; i++)
            {
                Destroy(carrommen[i]);
            }
        }

        public void OnDestroy()
        {
            DestroyExisting();
        }

        private static Vector2[] PlaceAlongPath(Vector2[] points, float radius)
        {
            var pointCount  = points.Length;
            var totalLength = 0.0f;
            float[] lengthUptoPoint = new float[pointCount];
            
            for (int i = 0, n = pointCount; i < n - 1; i++)
            {
                lengthUptoPoint[i] = totalLength;
                totalLength += (points[i] - points[i + 1]).magnitude;
            }

            lengthUptoPoint[pointCount - 1] = totalLength;

            var diameter = radius * 2;
            var result = new Vector2[Mathf.FloorToInt(totalLength / diameter) + 1];

            var covered = 0.0f;

            for (int i = 0, n = result.Length; i < n; i++)
            {
                covered = diameter * i;
                if (covered> totalLength) break;
                if (i == 0) covered = 0;

                var prevPoint = 0;
                var lenUptoPrev = lengthUptoPoint[prevPoint];
                while (lenUptoPrev <= covered)
                {
                    prevPoint++;
                    if (prevPoint >= pointCount)
                    {
                        prevPoint = pointCount - 1;
                        break;
                    }
                    lenUptoPrev = lengthUptoPoint[prevPoint];
                }

                var nextPoint = (prevPoint + 1) % pointCount;
                var lenUptoNext = lengthUptoPoint[nextPoint];

                var t = nextPoint == 0 ? 0 : (covered - lenUptoPrev) / (lenUptoNext - lenUptoPrev);
                result[i] = Vector2.Lerp(points[prevPoint], points[nextPoint], t);
            }

            return result;
        }

        public Line GetBaseline(PlayerNumber playerNumber, int totalPlayers)
        {
            return baselines[totalPlayers == 2 ? (playerNumber == PlayerNumber.Player1 ? 0 : 2) : ((int) playerNumber)];
        }
    }
}