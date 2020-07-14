using Carrom.UI;
using Networking.Foundation;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Carrom
{
    public class Striker : SliderLerp, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        #region Dragging and Shooting

        [Header("Scene References")]
        public new Rigidbody rigidbody;
        public StrikerUI strikerUi;
        public CarromGameplay gameplay;

        [Header("Shot Configuration")]
        public float noShootRadius = 0.05f;

        public float maxRadius  = 0.25f;
        public float flickForce = 4;

        #region SliderLerp
        private float _baselinePos;

        public override void SliderValueChanged(float value)
        {
            _baselinePos = value;
            if (SceneRegistry<CodeElementsType, CodeElements>.GetRandom(CodeElementsType.Board, gameObject.scene, out var codeElement)
            && codeElement is BoardSimulation board)
            {
                var baseline = board.GetBaseline(gameplay.AwaitingTurn, Match.Instance.PlayerCount);
                var position = baseline.GetPosition(value);
                rigidbody.position = position;
            }
        }
        #endregion

        public override void Awake()
        {
            strikerUi.transform.localPosition = Vector3.zero;
            strikerUi.gameObject.SetActive(false);
        }

        private Vector2 _beginDragPosition;
        private Vector3 _beginDragWorld;

        public void LateUpdate()
        {
            transform.position = rigidbody.position;
            transform.localScale = rigidbody.transform.lossyScale;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            strikerUi.gameObject.SetActive(true);
            strikerUi.Angle360 = 0.0f;
            strikerUi.Size01   = 0.0f;

            _beginDragPosition = eventData.position;
            _beginDragWorld    = eventData.WorldPosition();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            var direction = -eventData.DragDisplacementWorld(_beginDragWorld).normalized; //pull, not push
            var scale     = GetScale(eventData);

            if (scale <= 0.0f)
            {
                Debug.Log($"Not shot. Scale {scale} is less than minimum radius {noShootRadius}.");
            }

            DoShot(direction, scale);
            strikerUi.gameObject.SetActive(false);
        }

        public Vector3 scalexyz = Vector3.one;

        public void OnDrag(PointerEventData eventData)
        {
            var dragAngle = eventData.OppositeDragAngle(_beginDragPosition);
            strikerUi.Angle360 = dragAngle;

            strikerUi.Size01 = GetScale(eventData);
        }

        private float GetScale(PointerEventData eventData)
        {
            var dragDistance = eventData.DragDisplacementWorld(_beginDragWorld).magnitude;
            return dragDistance.Remap01(noShootRadius, maxRadius);
        }

        private void DoShot(Vector2 direction, float scale01)
        {
            Assert.IsNotNull(rigidbody, "Must have rigidbody on striker.");

            Match.Instance.SubmitShot(_baselinePos, Mathf.Atan2(direction.y, direction.x));
        }

        #endregion
    }
}