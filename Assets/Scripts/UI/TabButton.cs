using System.Data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utility;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public TabGroup tabGroup;

        public Image background;

        public UnityEvent onTabSelected;

        public UnityEvent onTabDeselected;

        public void Start()
        {
            gameObject.GetComponentIfNull(ref background);
            gameObject.GetComponentInParentIfNull(ref tabGroup);

            tabGroup.Subscribe(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tabGroup.OnTabEnter(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            tabGroup.OnTabExit(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            tabGroup.OnTabSelected(this);
        }

        public void Select()
        {
            onTabSelected?.Invoke();
        }

        public void Deselect()
        {
            onTabDeselected?.Invoke();
        }
    }
}