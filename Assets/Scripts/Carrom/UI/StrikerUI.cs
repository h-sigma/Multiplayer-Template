using System;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Assertions;

namespace Carrom.UI
{
    public interface DragUI
    {
        float Angle360 { get; set; }
        float Size01   { get; set; }
    }

    public class StrikerUI : MonoBehaviour, DragUI
    {
        #region DragUI

        private float _angle360;
        private float _size01;

        public float Angle360
        {
            get { return _angle360; }
            set
            {
                _angle360 = value;
                UpdateUI();
            }
        }

        public float Size01
        {
            get { return _size01; }
            set
            {
                _size01 = value;
                UpdateUI();
            }
        }

        #endregion

        public BoardSimulation simulation;

        public SphereCollider strikerCollider;
        public LayerMask      predictorMask;

        public Transform  manipulate;
        public GameObject validDrag;
        public GameObject invalidDrag;

        public LineRenderer lineRenderer;

        public Gradient stretchGradient;

        public float maxStretch;

        private SpriteRenderer _validDragRenderer;
        private SpriteRenderer _invalidDragRenderer;

        private void Awake()
        {
            Assert.IsNotNull(manipulate);
            Assert.IsNotNull(validDrag);
            Assert.IsNotNull(invalidDrag);
            Assert.IsNotNull(lineRenderer);

            _validDragRenderer   = validDrag.GetComponent<SpriteRenderer>();
            _invalidDragRenderer = invalidDrag.GetComponent<SpriteRenderer>();
        }

        private RaycastHit _hit;

        private void UpdateUI()
        {
            ManageValidityIndicator();
            ManageShotPredictor();

            void ManageValidityIndicator()
            {
                //set either arrow or cross active
                var valid = _size01 > 0.0f;

                validDrag.SetActive(valid);
                invalidDrag.SetActive(!valid);

                //if arrow (valid), change its color based on a gradient
                if (_validDragRenderer != null)
                    _validDragRenderer.color = stretchGradient.Evaluate(_size01);

                //manipulate scale and rotation of validity indicator
                manipulate.localScale = new Vector3(1 + _size01 * maxStretch, 1, 1);
                manipulate.rotation   = Quaternion.Euler(0, 0, _angle360);
            }

            void ManageShotPredictor()
            {
                var radius    = strikerCollider.gameObject.transform.lossyScale.x * strikerCollider.radius;
                var direction = new Vector3();
                direction.z = 0.0f;
                direction.x = Mathf.Cos(_angle360 * Mathf.Deg2Rad);
                direction.y = Mathf.Sin(_angle360 * Mathf.Deg2Rad);
                var origin = strikerCollider.bounds.center;

                lineRenderer.positionCount = 0;
                lineRenderer.startWidth    = radius / 4;
                lineRenderer.endWidth      = radius / 4;
                if (simulation.scene.SphereCast(origin, radius, direction, out _hit, Mathf.Infinity,
                    predictorMask.value))
                {
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPositions(new[]
                    {
                        origin, _hit.point
                    });
                }
            }
        }
    }
}