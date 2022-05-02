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
    public class PanelLevelComplete : MonoBehaviour
    {
        [Required]
        public Panel panel;
        [Required]
        public SimpleGrid holderCells;
        [Required, AssetsOnly]
        public PanelPlayerToolbarActionsCell cellPrefab;
        [Required]
        public Button buttonClose;
        public ParticlesList particlesOnOpen;

        private List<PanelPlayerToolbarActionsCell> cells = new List<PanelPlayerToolbarActionsCell>();
        private void Awake()
        {
            buttonClose.onClick.AddListener(HandleButtonCloseClick);
            panel.OnPanelShow += HandlePanelShow;
        }
        private void HandlePanelShow()
        {
            if (LevelController.Current == null)
                return;
            particlesOnOpen.Restart();
            UpdateView();
        }
        private void HandleButtonCloseClick()
        {
            particlesOnOpen.Stop();
            panel.ClosePanel();
        }
        private void UpdateView()
        {
            holderCells.transform.DestroyAllChilds();
            cells.Clear();

            foreach (Item itemInstance in LevelController.Current.levelStats.itemsLootInstances)
            {
                PanelPlayerToolbarActionsCell cell =
                    Instantiate(cellPrefab.gameObject, holderCells.transform)
                    .GetComponent<PanelPlayerToolbarActionsCell>();
                cell.SetItem(itemInstance);
                cell.interactable.Draggable = false;

                cells.Add(cell);
            }
            holderCells.UpdateView();
        }
    }
}
