using Pixeye.Actors;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using static TeamAlpha.Source.LayerDefault;

namespace TeamAlpha.Source
{
    public class NPC : MonoBehaviour
    {
        public enum Type { Melee, Ranged }

        [Required]
        public Character character;
        [ShowInInspector]
        public int Level
        {
            get
            {
                int result =
                    character.lvl3StatsResultSum.main.StatsSum
                    - DataGameMain.Default.skillPointsNPCFirstLevel;
                result /= DataGameMain.Default.skillPointsPerNPCLevel;
                result++;
                result = Mathf.Clamp(result, 1, result);

                return result;
            }
        }
        [MinValue(0)]
        public int rewardExperience;
        public Type type;

        [ReadOnly]
        public Vector2Int gridPositionFromBottom;
        public Vector2Int GridPositionFromTop
        {
            get
            {
                Vector2Int result = gridPositionFromBottom;
                Vector2Int sizeDiff = LevelController.Current.sizeBattlefield;
                sizeDiff.x = 0;
                sizeDiff.y -= 4;
                result.y = LevelController.Current.sizeBattlefield.y - gridPositionFromBottom.y - 1;
                //if (sizeDiff.y > 0)
                result.y -= sizeDiff.y;
                return result;
            }
        }

        public int CooldownLeft
        {
            get => cooldownLeft;
            set
            {
                cooldownLeft = value;
                PanelBattleGrid.Default.UpdateView();
            }
        }
        private int cooldownLeft;
        public bool EnemyInRange
        {
            get
            {
                bool result =
                (type == NPC.Type.Melee && gridPositionFromBottom.y == 0) ||
                    (type == NPC.Type.Ranged && gridPositionFromBottom.y <= 1);

                return result;
            }
        }
        public bool CanAttack => EnemyInRange && CooldownLeft == 0;
        [NonSerialized]
        public bool firstTurnPassed;

        public void DoAttackPlayer()
        {
            List<Character> enemies = new List<Character>();
            if (LevelController.Current.allies.Contains(character))
                enemies.AddRange(LevelController.Current.enemies.ToArray());
            else
                enemies.AddRange(LevelController.Current.allies.ToArray());

            Character target = enemies.Random();

            AbilitySimpleAttack simpleAttack =
                character.abilities.Find(a => a is AbilitySimpleAttack) as AbilitySimpleAttack;
            if (simpleAttack.CanBeActivatedOnTarget(target))
                simpleAttack.Activate(target);
            //PanelBattleGrid.Default.HandleCharacterDoAttack(character);
        }
        public StatsResources CalculateDamageToPlayer()
        {
            AbilitySimpleAttack simpleAttack =
                character.abilities.Find(a => a is AbilitySimpleAttack) as AbilitySimpleAttack;
            return simpleAttack.GetAllDamage(false);
        }
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;

            RectTransform rectTransform = PanelBattleGrid.Default.GetCellByCharacter(character).GetComponent<RectTransform>();
            Bounds bounds = new Bounds(rectTransform.position, rectTransform.rect.size);
            Vector3 min = rectTransform.position - (Vector3)rectTransform.rect.size / 2f;
            Vector3 max = rectTransform.position + (Vector3)rectTransform.rect.size / 2f;

            bounds.Encapsulate(min);
            bounds.Encapsulate(max);
            DebugExtension.DrawBounds(bounds, Color.cyan);
        }
#endif
        public List<CharacteristicDescription> GetDescription()
        {
            List<CharacteristicDescription> result = new List<CharacteristicDescription>();

            CharacteristicDescription characteristicCooldownLeft = new CharacteristicDescription();
            characteristicCooldownLeft.valueToCompare = CooldownLeft;
            string descriptionCooldownLeft = "";
            if (CooldownLeft == 0)
                descriptionCooldownLeft = "Agressive";
            else if (cooldownLeft == 1)
                descriptionCooldownLeft = "Becomes aggressive in the next round";
            else if (CooldownLeft >= 2)
                descriptionCooldownLeft = "Becomes aggressive soon";

            characteristicCooldownLeft.description = descriptionCooldownLeft;
            //characteristicCooldownLeft.name = "Level";
            result.Add(characteristicCooldownLeft);

            CharacteristicDescription characteristicLevel = new CharacteristicDescription();
            characteristicLevel.valueToCompare = Level;
            characteristicLevel.description = "Level " + characteristicLevel.valueToCompare.ToString();
            characteristicLevel.nameToCompare = "Level";
            result.Add(characteristicLevel);

            CharacteristicDescription characteristicType = new CharacteristicDescription();
            characteristicType.valueToCompare = (int)type;
            characteristicType.description = "Type " + type.ToString();
            characteristicType.nameToCompare = "Type";
            result.Add(characteristicType);

            CharacteristicDescription characteristicHealth = new CharacteristicDescription();
            characteristicHealth.valueToCompare = Mathf.CeilToInt(character.statsResources.healthCur);
            characteristicHealth.description = "Health " + characteristicHealth.valueToCompare.ToString();
            characteristicHealth.nameToCompare = "Health";
            result.Add(characteristicHealth);

            CharacteristicDescription characteristicDamage = new CharacteristicDescription();
            characteristicDamage.valueToCompare = Mathf.CeilToInt(character.lvl3StatsResultSum.common.damage);
            characteristicDamage.description = "Damage " + characteristicDamage.valueToCompare.ToString();
            characteristicDamage.nameToCompare = "Damage";
            result.Add(characteristicDamage);

            return result;
        }
    }
}