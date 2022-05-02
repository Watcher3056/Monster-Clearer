using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelBattleGrid : MonoBehaviour
    {
        public static PanelBattleGrid Default { get; private set; }

        [Required]
        public Panel panel;
        [Required, AssetsOnly]
        public CharacterCell prefabCell;
        [Required]
        public SimpleGrid holder;
        [Required]
        public Sprite spriteStatusDead;
        [Required]
        public Sprite spriteStatusAttack;
        [Required]
        public Sprite spriteStatusCooldown1;
        [Required]
        public Sprite spriteStatusCooldown2;
        [Required]
        public Image imageBG;
        [Required]
        public CharacterCellPlayer cellPlayer;
        public float animSpeed = 2f;


        private List<CharacterCell> cells = new List<CharacterCell>();
        public PanelBattleGrid() => Default = this;
        private void Awake()
        {
            holder.transform.DestroyAllChilds();
        }
        public void HandleAddEnemy(NPC npc)
        {
            CharacterCell cell =
                Instantiate(prefabCell.gameObject, holder.transform).GetComponent<CharacterCell>();

            cell.SetCharacter(npc.character);
            cell.interactable.OnClick += () => HandleClickNPC(npc);
            
            cells.Add(cell);
        }
        private void HandleClickNPC(NPC npc)
        {
            if (!npc.character.IsAlive)
                return;
            PanelContextInfo.Default.ShowInfo(GetCellByCharacter(npc.character) as CharacterCellNPC);
        }
        public void HandleRemoveEnemy(NPC npc)
        {
            CharacterCell cell = cells.Find(c => c.LinkedCharacter == npc);
            cell.ClearCharacter();
            cells.Remove(cell);
            Destroy(cell.gameObject);
        }
        public void Clear()
        {
            foreach (CharacterCell cell in cells)
                Destroy(cell.gameObject);
            cells.Clear();
        }
        public void UpdateView()
        {
            foreach (CharacterCellNPC cell in cells)
            {
                cell.UpdateView(animSpeed);

                holder.multiPaged = false;
                holder.columnCount = LevelController.Current.sizeBattlefield.x;
                holder.rowCount = LevelController.Current.sizeBattlefield.y - 1;
                Vector3 position =
                    holder.GetPosition(cell.LinkedNPC.GridPositionFromTop.x, cell.LinkedNPC.GridPositionFromTop.y);
                if (cell.LinkedNPC.gridPositionFromBottom.y > LevelController.Current.sizeBattlefield.y - 1)
                    position.y = 
                        holder.offsetByCellY * 
                        (cell.LinkedNPC.gridPositionFromBottom.y + 1 - 
                        LevelController.Current.sizeBattlefield.y);
                else if (cell.LinkedNPC.gridPositionFromBottom.y == 0)
                    position.y += holder.offsetByPage.y;
                //cell.transform.DOKill();
                cell.GetComponent<RectTransform>().DOAnchorPos(position, 1f / animSpeed);

                if (cell.LinkedNPC.EnemyInRange)
                {
                    cell.imageIndicatorStatus.gameObject.SetActive(true);
                    if (cell.LinkedNPC.CooldownLeft == 2)
                        cell.imageIndicatorStatus.sprite = spriteStatusCooldown2;
                    else if (cell.LinkedNPC.CooldownLeft == 1)
                        cell.imageIndicatorStatus.sprite = spriteStatusCooldown1;
                    else if (cell.LinkedNPC.CooldownLeft == 0)
                        cell.imageIndicatorStatus.sprite = spriteStatusAttack;
                }
                else
                {
                    cell.imageIndicatorStatus.gameObject.SetActive(false);
                }
            }
        }
        public void HandleNPCDied(NPC npc)
        {
            CharacterCellNPC cell = cells.Find(c => c.LinkedCharacter == npc.character) as CharacterCellNPC;
            cells.Remove(cell);
            cell.viewDead.SetActive(true);
            cell.imageIndicatorStatus.sprite = spriteStatusDead;
            cell.UpdateView(animSpeed);
            //cell.transform.DOKill();
            cell.transform.DOScale(Vector3.zero, 1f / animSpeed)
                .SetDelay(0.25f)
                .onComplete = () =>
            {
                HandleRemoveEnemy(cell.LinkedNPC);
            };
        }
        public void HandleCharacterDoAttack(Character character)
        {
            CharacterCell cell = GetCellByCharacter(character);
            //cell.transform.DOKill();
            cell.transform.DOScale(1.25f, 0.25f).onComplete = () =>
            {
                cell.transform.DOScale(1f, 0.25f);
            };
        }
        public void HandleCharacterAttacked(Character target)
        {
            CharacterCell cellTarget = GetCellByCharacter(target);

            cellTarget.HandleDamageReceived();
        }
        public CharacterCell GetCellByCharacter(Character character)
        {
            CharacterCell result = null;
            if (character == PlayerController.Current.character)
                result = cellPlayer;
            else
                result = cells.Find(c => c.LinkedCharacter == character);
            return result;
        }
        public void SetActiveAbilityRangeIndicatorByCharacter(Character character, bool arg)
        {
            SetActiveAbilityRangeIndicator(GetCellByCharacter(character), arg);
        }
        public void SetActiveAbilityRangeIndicator(CharacterCell cell, bool arg)
        {
            cell.SetActiveAbilityRangeIndicator(arg);
        }
        public void ResetAllAbilityRangeIndicators()
        {
            foreach (CharacterCell cell in cells)
                SetActiveAbilityRangeIndicator(cell, false);
            cellPlayer.SetActiveAbilityRangeIndicator(false);
        }
    }
}
