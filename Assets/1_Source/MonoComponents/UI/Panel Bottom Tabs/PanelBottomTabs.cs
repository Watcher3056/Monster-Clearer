using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelBottomTabs : MonoBehaviour
    {
        public static PanelBottomTabs Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public PanelBottomTab tabShop;
        [Required]
        public PanelBottomTab tabMap;
        [Required]
        public PanelBottomTab tabInventory;
        [Required]
        public PanelBottomTab tabSettings;
        [Required]
        public RectTransform holderTabs;
        public Vector2 tabsSizeNormal = new Vector2(250, 130);
        public Vector2 tabsSizeSelected = new Vector2(300, 150);
        public float animSpeed = 4f;

        private PanelBottomTab curTab;
        private List<PanelBottomTab> tabs = new List<PanelBottomTab>();

        public PanelBottomTabs() => Default = this;
        private void Awake()
        {
            tabs.Add(tabShop);
            tabs.Add(tabMap);
            tabs.Add(tabInventory);
            tabs.Add(tabSettings);


            foreach (PanelBottomTab tab in tabs)
            {
                PanelBottomTab _tab = tab;
                _tab.button.onClick.AddListener(() => HandleTabSelected(_tab));
            }
            ProcessorObserver.Default.Add(() => UIManager.Default.CurState, curState => HandleUIManagerStateChanged(), true);
        }
        private void HandleUIManagerStateChanged()
        {
            if (UIManager.Default.CurState == UIManager.State.MainMenu)
                HandleTabSelected(tabMap);
            else if (UIManager.Default.CurState == UIManager.State.Inventory)
                HandleTabSelected(tabInventory);
            else if (UIManager.Default.CurState == UIManager.State.Settings)
                HandleTabSelected(tabSettings);
            else
                HandleTabSelected(null);
        }
        private void HandleTabSelected(PanelBottomTab tab)
        {
            if (curTab != null)
            {
                curTab.rectTransform.DOKill();
                curTab.rectTransform
                    .DOSizeDelta(tabsSizeNormal, 1f / animSpeed)
                    .OnUpdate(() =>
                    {
                        LayoutRebuilder.MarkLayoutForRebuild(holderTabs);
                    });
            }
            curTab = tab;

            if (curTab == null)
                return;
            curTab.rectTransform.DOKill();
            curTab.rectTransform
                .DOSizeDelta(tabsSizeSelected, 1f / animSpeed)
                .OnUpdate(() =>
                {
                    LayoutRebuilder.MarkLayoutForRebuild(holderTabs);
                });

            if (curTab == tabInventory)
            {
                UIManager.Default.CurState = UIManager.State.Inventory;
            }
            else if (curTab == tabMap)
            {
                UIManager.Default.CurState = UIManager.State.MainMenu;
            }
            else if (curTab == tabSettings)
            {
                UIManager.Default.CurState = UIManager.State.Settings;
            }
        }
    }
}
