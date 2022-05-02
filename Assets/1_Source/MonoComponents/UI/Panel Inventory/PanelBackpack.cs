using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class PanelBackpack : MonoBehaviour
    {
        public static PanelBackpack Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public SimpleGrid holderCells;
        [Required]
        public PanelBackpackDisposerChest disposerChest;

        [NonSerialized]
        public List<PanelPlayerToolbarActionsCell> cells = new List<PanelPlayerToolbarActionsCell>();
        [NonSerialized]
        public List<Item> itemsCheckedByPlayer = new List<Item>();

        public PanelBackpack() => Default = this;
        private void Awake()
        {
            cells = holderCells.gameObject.GetAllComponents<PanelPlayerToolbarActionsCell>(false);
            foreach (PanelPlayerToolbarActionsCell cell in cells)
            {
                PanelEquipment.Default.RegisterCell(cell);
            }

            PlayerController.Current.character.OnBackpackChanged += UpdateContent;
            PlayerController.Current.character.OnItemEquipedOrUnequiped += UpdateContent;
        }
        private void UpdateContent()
        {
            itemsCheckedByPlayer.RemoveAll(i => !PlayerController.Current.character.Contains(i));
            foreach (Character.EquipmentSlot s in PlayerController.Current.character.equipmentSlots)
                if (s.equipedItem != null)
                    MarkItemAsNew(s.equipedItem, false, false);

            for (int i = 0; i < cells.Count; i++)
            {
                PanelPlayerToolbarActionsCell cell = cells[i];
                if (i >= PlayerController.Current.character.backpack.Count)
                    cell.Clear();
                else
                {
                    cell.SetItem(PlayerController.Current.character.backpack[i]);
                    if (itemsCheckedByPlayer.Contains(cell.linkedItem))
                        cell.MarkContentAsNew = false;
                    else
                        cell.MarkContentAsNew = true;
                }
            }
        }
        public void MarkItemAsNew(Item item, bool arg, bool updateContent = true)
        {
            if (arg)
                itemsCheckedByPlayer.Remove(item);
            else if (!itemsCheckedByPlayer.Contains(item))
                itemsCheckedByPlayer.Add(item);

            if (updateContent)
                UpdateContent();
        }
    }
}
