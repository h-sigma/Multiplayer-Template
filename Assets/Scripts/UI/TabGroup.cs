using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class TabGroup : MonoBehaviour
    {
        [NonSerialized]
        public List<TabButton> TabButtons;

        public Sprite tabIdle;
        public Sprite tabHover;
        public Sprite tabActive;

        public List<GameObject> objectsToSwap;

        [NonSerialized]
        public TabButton SelectedTab;

        public void Subscribe(TabButton button)
        {
            if(TabButtons == null) TabButtons = new List<TabButton>();

            TabButtons.Add(button);
        }

        public void OnTabEnter(TabButton button)
        {
            ResetTabsLook();
            if (!IsSelected(button))
            {
                button.background.sprite = tabHover;
            }
        }

        public void OnTabExit(TabButton button)
        {
            ResetTabsLook();
        }

        public void OnTabSelected(TabButton button)
        {
            if (SelectedTab != null)
            {
                SelectedTab.Deselect();
            }
            
            SelectedTab = button;
            
            SelectedTab.Select();
            
            ResetTabsLook();
            button.background.sprite = tabActive;

            int index = button.transform.GetSiblingIndex();
            for (int i = 0; i < objectsToSwap.Count; i++)
            {
                if (index == i)
                {
                    objectsToSwap[i].SetActive(true);
                }
                else
                {
                    objectsToSwap[i].SetActive(false);
                }
            }
        }

        private bool IsSelected(TabButton button)
        {
            return SelectedTab != null && SelectedTab != button;
        }

        public void ResetTabsLook()
        {
            foreach (var tabButton in TabButtons)
            {
                if (IsSelected(tabButton)) continue;
                tabButton.background.sprite = tabIdle;
            }
        }
    }
}
