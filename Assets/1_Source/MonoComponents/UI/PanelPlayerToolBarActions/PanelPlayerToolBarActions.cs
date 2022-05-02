using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using static TeamAlpha.Source.Character;

namespace TeamAlpha.Source
{
    public class PanelPlayerToolBarActions : MonoBehaviour
    {
        public static PanelPlayerToolBarActions Default { get; private set; }

        [Required]
        public Panel panel;
        //[Required, AssetsOnly]
        //public PanelPlayerToolbarActionsCell cellPrefabSpells;
        //[Required, AssetsOnly]
        //public PanelPlayerToolbarActionsCell cellPrefabConsumables;
        //[Required, AssetsOnly]
        //public PanelPlayerToolbarActionsCell cellPrefabWeapons;
        [Required]
        public SimpleGrid holderSpells;
        [Required]
        public SimpleGrid holderExpendable;
        [Required]
        public SimpleGrid holderWeapons;

        [NonSerialized]
        public List<PanelPlayerToolbarActionsCell> allCells = new List<PanelPlayerToolbarActionsCell>();
        private List<PanelPlayerToolbarActionsCell> cellsAbilities = new List<PanelPlayerToolbarActionsCell>();
        private List<PanelPlayerToolbarActionsCell> cellsExpendable = new List<PanelPlayerToolbarActionsCell>();
        private List<PanelPlayerToolbarActionsCell> cellsWeapons = new List<PanelPlayerToolbarActionsCell>();
        private CharacterCell curTargetCell;
        private List<Character> charactersInRange = new List<Character>();

        public PanelPlayerToolBarActions() => Default = this;
        private void Awake()
        {
            cellsAbilities.AddRange(holderSpells.gameObject.GetAllComponents<PanelPlayerToolbarActionsCell>(false));
            cellsExpendable.AddRange(holderExpendable.gameObject.GetAllComponents<PanelPlayerToolbarActionsCell>(false));
            cellsWeapons.AddRange(holderWeapons.gameObject.GetAllComponents<PanelPlayerToolbarActionsCell>(false));
            allCells.AddRange(cellsAbilities.ToArray());
            allCells.AddRange(cellsExpendable.ToArray());
            allCells.AddRange(cellsWeapons.ToArray());
            //holderSpells.transform.DestroyAllChilds();
            //holderConsumables.transform.DestroyAllChilds();
            //holderWeapons.transform.DestroyAllChilds();

            foreach (PanelPlayerToolbarActionsCell cell in allCells)
            {
                PanelPlayerToolbarActionsCell _cell = cell;
                cell.interactable.OnDragStart += () => HandleCellDragStart(_cell);
                cell.interactable.OnDragEnd += () => HandleCellDragEnd(_cell);
            }

            PlayerController.Current.character.OnAbilitiesChanged += UpdateContent;
            PlayerController.Current.character.OnItemEquipedOrUnequiped += UpdateContent;
            PlayerController.Current.character.OnBackpackChanged += UpdateContent;

            UpdateContent();
        }
        private void Update()
        {
            if (PlayerController.Current.CellDraggingNow == null)
                return;
            HandleCellPointing();
        }
        private void HandleCellDragStart(PanelPlayerToolbarActionsCell cell)
        {

        }
        private void HandleCellDragEnd(PanelPlayerToolbarActionsCell cell)
        {
            PlayerController.Current.CellDraggingNow = null;
            PanelBattleGrid.Default.ResetAllAbilityRangeIndicators();
            if (curTargetCell != null && cell.ContentCanBeUsedOnTarget(curTargetCell.LinkedCharacter))
            {
                foreach (Character character in charactersInRange)
                    PanelBattleGrid.Default.HandleCharacterAttacked(character);
                cell.UseContentOnTarget(curTargetCell.LinkedCharacter);
                UpdateView();
            }
            curTargetCell = null;

            cell.Interactable = false;
            cell.interactable.transform.DOScale(0f, 0.25f).onComplete = () =>
            {
                cell.interactable.transform.localPosition = Vector3.zero;
                cell.interactable.transform.DOScale(1f, 0.25f).onComplete = () =>
                {
                    cell.Interactable = true;
                    cell.SetActiveCellMask(true);
                    UIManager.Default.SetHighlightUIElement(cell.interactable.gameObject, false);
                };
            };
        }
        public void UpdateContent()
        {
            List<Ability> abilitiesToAdd = new List<Ability>(PlayerController.Current.character.abilities.ToArray());
            abilitiesToAdd.Remove(PlayerController.Current.AbilityDefaultAttack);
            for (int i = 0; i < cellsAbilities.Count; i++)
            {
                PanelPlayerToolbarActionsCell cell = cellsAbilities[i];
                if (i >= abilitiesToAdd.Count)
                    cell.Clear();
                else
                {
                    Ability ability = abilitiesToAdd[i];
                    cell.SetAbility(ability);
                }
            }

            List<EquipmentSlot> slots = new List<EquipmentSlot>();
            slots.AddRange(PlayerController.Current.character.equipmentSlots.ToArray());
            foreach (PanelPlayerToolbarActionsCell cell in allCells)
            {
                cell.SetActiveCellMask(true);
                cell.interactable.Draggable = true;
                UIManager.Default.SetHighlightUIElement(cell.interactable.gameObject, false);
                if (cell.contentType != PanelPlayerToolbarActionsCell.ContentType.Item)
                    continue;
                EquipmentSlot slot = slots.Find(s => s.type == Item.Type.Any || s.type == cell.itemType);
                Item item = slot?.equipedItem;
                if (item != null)
                {
                    cell.SetItem(item);
                }
                else
                    cell.Clear();
                slots.Remove(slot);
            }
        }
        public void UpdateView()
        {
            foreach (PanelPlayerToolbarActionsCell cell in allCells)
                cell.UpdateView();
        }
        private void UpdateAbilityRangeIndicators()
        {
            foreach (Character character in charactersInRange)
                PanelBattleGrid.Default.SetActiveAbilityRangeIndicatorByCharacter(character, true);
        }
        private void HandleCellPointing()
        {
            PanelBattleGrid.Default.ResetAllAbilityRangeIndicators();
            Vector2 pos = PlayerController.Current.GetPointerScreenPosition();
            curTargetCell = GetCharacterCellByScreenPos(pos);
            //curTargetCell = GetCharacterCellByScreenPos(Input.mousePosition);
            if (curTargetCell != null && PlayerController.Current.CellDraggingNow.ContentCanBeUsedOnTarget(curTargetCell.LinkedCharacter))
            {
                charactersInRange = PlayerController.Current.CellDraggingNow.linkedAbility
                    .GetAllCharactersInRangeFromGrid(curTargetCell.LinkedCharacter);

                UpdateAbilityRangeIndicators();
            }
        }
        public void HandleAbilityPointingByScreenPosition(Vector3 position, Ability ability)
        {
            PanelBattleGrid.Default.ResetAllAbilityRangeIndicators();
            curTargetCell = GetCharacterCellByScreenPos(position);
            if (curTargetCell != null && ability.CanBeActivatedOnTarget(curTargetCell.LinkedCharacter))
            {
                charactersInRange = ability
                    .GetAllCharactersInRangeFromGrid(curTargetCell.LinkedCharacter);
                UpdateAbilityRangeIndicators();
            }
        }
        public void HandleUseAbilityByScreenPosition(Vector3 position, Ability ability)
        {
            HandleAbilityPointingByScreenPosition(position, ability);
            if (curTargetCell == null)
                return;
            if (!ability.CanBeActivatedOnTarget(curTargetCell.LinkedCharacter))
                return;
            PanelBattleGrid.Default.ResetAllAbilityRangeIndicators();

            foreach (Character character in charactersInRange)
                PanelBattleGrid.Default.HandleCharacterAttacked(character);
            ability.Activate(curTargetCell.LinkedCharacter);
        }
        public void SetActivePlayerControls(bool arg)
        {
            foreach (PanelPlayerToolbarActionsCell cell in allCells)
            {
                cell.Interactable = arg;
            }
            PanelBattleGrid.Default.cellPlayer.Interactable = arg;
        }
        public CharacterCell GetCharacterCellByScreenPos(Vector3 screenPos)
        {
            return UIManager.Default.Raycast<CharacterCell>(screenPos);
        }
    }
}
