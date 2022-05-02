using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static TeamAlpha.Source.Character;

namespace TeamAlpha.Source
{
    public class PanelEquipment : MonoBehaviour
    {
        public static PanelEquipment Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public Transform holderCells;
        [Required]
        public CharacterCellPlayer cellPlayer;
        public float dragSnapRange = 2f;

        [NonSerialized]
        public List<PanelEquipmentCell> cells = new List<PanelEquipmentCell>();
        [NonSerialized]
        public List<PanelPlayerToolbarActionsCell> allCells = new List<PanelPlayerToolbarActionsCell>();
        public PanelEquipment() => Default = this;
        private void Awake()
        {
            cells = holderCells.gameObject.GetAllComponents<PanelEquipmentCell>(false);
            List<EquipmentSlot> slots = new List<EquipmentSlot>();
            slots.AddRange(PlayerController.Current.character.equipmentSlots.ToArray());
            foreach (PanelEquipmentCell cell in cells)
            {
                RegisterCell(cell);

                EquipmentSlot slot = slots.Find(s => s.type == cell.itemType);
                cell.LinkedSlot = slot;
                slots.Remove(slot);
            }

            PlayerController.Current.character.OnItemEquipedOrUnequiped += UpdateContent;
            PlayerController.Current.character.OnBackpackChanged += UpdateContent;
        }
        public void RegisterCell(PanelPlayerToolbarActionsCell cell)
        {
            allCells.Add(cell);
            PanelPlayerToolbarActionsCell _cell = cell;
            cell.interactable.OnDragStart += () => HandleCellDragStart(_cell);
            cell.interactable.OnDragEnd += () => HandleCellDragEnd(_cell);
            cell.interactable.OnClick += () => HandleCellClick(_cell);
        }
        private void HandleCellClick(PanelPlayerToolbarActionsCell cell)
        {
            if (PanelBackpack.Default.cells.Contains(cell))
            {
                PanelBackpack.Default.MarkItemAsNew(cell.linkedItem, false);
                List<PanelPlayerToolbarActionsCell> cellsToCompare = new List<PanelPlayerToolbarActionsCell>();
                cellsToCompare
                    .AddRange(cells.FindAll(c => c.linkedItem != null && c.itemType == cell.linkedItem.type).ToArray());

                PanelContextInfo.Default.ShowComparison(cell, cellsToCompare);
            }
        }
        private void HandleCellDragStart(PanelPlayerToolbarActionsCell cell)
        {

        }
        private void HandleCellDragEnd(PanelPlayerToolbarActionsCell cell)
        {
            void MoveCellContentBack(PanelPlayerToolbarActionsCell cell)
            {
                cell.interactable.transform
                    .DOMove(cell.transform.position, 0.25f)
                    .OnComplete(() =>
                    {
                        cell.interactable.transform.localPosition = Vector3.zero;
                        cell.Interactable = true;
                        cell.SetActiveCellMask(true);
                        UIManager.Default.SetHighlightUIElement(cell.interactable.gameObject, false);
                    });
            }
            void MoveCellContent(PanelPlayerToolbarActionsCell cellFrom, PanelPlayerToolbarActionsCell cellTo)
            {
                cellFrom.interactable.transform
                    .DOMove(cellTo.transform.position, 0.25f)
                    .OnComplete(() =>
                    {
                        PanelBackpack.Default.MarkItemAsNew(cellFrom.linkedItem, false);
                        if (PanelBackpack.Default.cells.Contains(cellTo))
                        {
                            PlayerController.Current.character.UnEquipItem(cellFrom.linkedItem);
                        }
                        else if (cells.Contains(cellTo))
                            PlayerController.Current.character
                            .EquipItem(cellFrom.linkedItem, (cellTo as PanelEquipmentCell).LinkedSlot);
                    });
            }
            Vector3 searchFrom = CameraManager.Default.cam
                .ScreenToWorldPoint(Input.mousePosition);

            List<PanelPlayerToolbarActionsCell> bestCells =
                allCells.FindAll(c => c.itemType == Item.Type.Any || c.itemType == cell.linkedItem.type);
            PanelPlayerToolbarActionsCell nearestEmpryCell =
                searchFrom
                .FindNearestFromPoint(bestCells);
            if (nearestEmpryCell != null &&
                Vector2.Distance(nearestEmpryCell.transform.position, searchFrom) > dragSnapRange)
                nearestEmpryCell = null;

            cell.Interactable = false;

            if (PanelBackpack.Default.disposerChest.Hover && PanelBackpack.Default.disposerChest.FillProgress < 1f)
            {
                PanelBackpack.Default.disposerChest.HandleItemDispose(cell);
            }
            else if (nearestEmpryCell != null)
            {
                if (PanelBackpack.Default.cells.Contains(nearestEmpryCell))
                {
                    if (!PanelBackpack.Default.cells.Contains(cell))
                    {
                        nearestEmpryCell = PanelBackpack.Default.cells.Find(c => !c.HasContent);
                        if (nearestEmpryCell != null)
                            MoveCellContent(cell, PanelBackpack.Default.cells[0]);
                        else
                            MoveCellContentBack(cell);
                    }
                    else
                        MoveCellContentBack(cell);
                }
                else if (cell.linkedItem.type == (nearestEmpryCell as PanelEquipmentCell).LinkedSlot.type)
                    MoveCellContent(cell, nearestEmpryCell);
                else
                    MoveCellContentBack(cell);
            }
            else
            {
                MoveCellContentBack(cell);
            }
            PlayerController.Current.CellDraggingNow = null;
        }
        private void UpdateContent()
        {
            for (int i = 0; i < cells.Count; i++)
            {
                PanelEquipmentCell cell = cells[i];
                if (cell.LinkedSlot.equipedItem != null)
                    cell.SetItem(cell.LinkedSlot.equipedItem);
                else
                    cell.Clear();
            }
            foreach (PanelPlayerToolbarActionsCell cell in allCells)
            {
                cell.Interactable = true;
                cell.interactable.Draggable = true;
                cell.interactable.transform.localPosition = Vector3.zero;
                cell.interactable.transform.localScale = Vector3.one;
                cell.SetActiveCellMask(true);
                UIManager.Default.SetHighlightUIElement(cell.interactable.gameObject, false);
            }
        }
    }
}
