using DG.Tweening;
using Pixeye.Actors;
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
    public class PanelChestReward : MonoBehaviour
    {
        public static PanelChestReward Default { get; private set; }
        [Required]
        public Panel panel;
        [Required]
        public RectTransform holderCells;
        [Required]
        public Button buttonGetReward;
        public float animSpeed = 4f;

        private List<PanelPlayerToolbarActionsCell> cells = new List<PanelPlayerToolbarActionsCell>();
        public PanelPlayerToolbarActionsCell CellSelected { get; private set; }

        public PanelChestReward() => Default = this;
        private void Awake()
        {
            buttonGetReward.onClick.AddListener(HandleButtonGetRewardClick);

            cells = holderCells.gameObject.GetAllComponents<PanelPlayerToolbarActionsCell>(false);

            foreach (PanelPlayerToolbarActionsCell cell in cells)
            {
                PanelPlayerToolbarActionsCell _cell = cell;
                cell.interactable.OnClick += () => HandleCellClick(_cell);
            }
        }
        public void ShowReward()
        {
            panel.OpenPanel();
            buttonGetReward.interactable = false;
            CellSelected = null;

            List<Item> itemsPrefabs =
                LayerDefault.Default.GetLastLevelToPlayOrFinal().GetLootPrefabs(cells.Count);
            for (int i = 0; i < cells.Count; i++)
            {
                PanelPlayerToolbarActionsCell cell = cells[i];
                cell.transform.localScale = Vector3.one;
                cell.SetItem(itemsPrefabs[i].InstantiateFromOrigin(true));
                cell.interactable.Draggable = false;
            }
        }
        private void HandleCellClick(PanelPlayerToolbarActionsCell cellClicked)
        {
            CellSelected = cellClicked;

            foreach (PanelPlayerToolbarActionsCell cell in cells)
            {
                if (cell == CellSelected)
                {
                    cell.viewLocked.gameObject.SetActive(false);
                    cell.transform.DOScale(1.15f, 1f / animSpeed);
                }
                else
                {
                    cell.viewLocked.gameObject.SetActive(true);
                    cell.transform.DOScale(0.9f, 1f / animSpeed);
                }
            }

            buttonGetReward.interactable = true;
        }
        private void HandleButtonGetRewardClick()
        {
            panel.ClosePanel();
            PlayerController.Current.character.AddItemToBackpack(CellSelected.linkedItem, true);
            PlayerController.Current.BackpackDisposeChestAmount = 0;

            for (int i = 0; i < cells.Count; i++)
            {
                PanelPlayerToolbarActionsCell cell = cells[i];
                if (cell != CellSelected)
                    Destroy(cell.linkedItem.gameObject);
                cell.Clear();
            }
        }
    }
}
