using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamAlpha.Source
{
    public partial class Ability : MonoBehaviour
    {
        #region Data Containers
        public enum TargetRelationType { Allies, Enemy, Any }
        #endregion

        #region General Config
        [HideLabel, Title("Component GUID")]
        public ComponentGUID componentGUID;
        [Required, PreviewField(100f)]
        public Sprite spriteIcon;
        [Required]
        public string abilityName;
        [Required]
        public string abilityDescription;
        public AbilityExtensionVisual extensionVisual;
        public bool limitDistance;
        [ShowIf("limitDistance")]
        public int maxDistance;
        public bool spendTurn;
        public bool multiTarget;
#if UNITY_EDITOR
        [TableMatrix(HorizontalTitle = "Hit Range Shape",
            DrawElementMethod = "DrawColoredEnumElement", SquareCells = true,
            ResizableColumns = false, RowHeight = 16), ShowIf("multiTarget"), ShowInInspector,
            OnValueChanged("_EditorOnMatrixMultiTargetSetupValueChanged")]
        public bool[,] editorMatrixMultiTargetSetup;
        [HideInInspector]
        public Vector2Int matrixMultiTargetSetupSize;
#endif
        public TargetRelationType targetRelationType;

        public StatsResources cost;
        //[MinValue(0)]
        //public float executionTime;
        [MinValue(0)]
        public int cooldownTime;

        [HideInInspector, SerializeField]
        public List<Vector2Int> multiTargetSetupList = new List<Vector2Int>();
        #endregion

        #region RUNTIME
        public int RemainCooldown { get; set; }
        public Character Owner { get; set; }
        public float Power
        {
            get => power;
            set => power = value;
        }
        private float power = 1f;
        #endregion
        public bool CanBeActivatedOnTarget(Character target)
        {
            bool teamFilterPassed = false;
            if (targetRelationType == TargetRelationType.Any)
                teamFilterPassed = true;
            else if (targetRelationType == TargetRelationType.Allies)
                teamFilterPassed = Owner.CurTeam == target.CurTeam;
            else if (targetRelationType == TargetRelationType.Enemy)
                teamFilterPassed = Owner.CurTeam != target.CurTeam;

            return 
                CanBeActivated() && 
                HandleCanBeActivatedOnTarget(target) && 
                teamFilterPassed &&
                TargetInRange(target);
        }
        public bool CanBeActivated()
        {
            return
                Owner.statsResources.healthCur >= cost.healthCur &&
                Owner.statsResources.staminaCur >= cost.staminaCur &&
                !IsOnCooldown(this, Owner) &&
                HandleCanBeActivated();
        }
        public void Activate(Character target)
        {
            RemainCooldown = cooldownTime;

            Owner.statsResources -= cost;

            if (multiTarget && target.CurTeam == Character.Team.NPC)
            {
                List<Character> charactersInRange = GetAllCharactersInRangeFromGrid(target);
                foreach (Character character in charactersInRange)
                {
                    bool isSecondaryTarget = character == target ? false : true;
                    if (extensionVisual != null)
                        extensionVisual.Activate(Owner, target, isSecondaryTarget);
                    HandleActivate(character, isSecondaryTarget);
                }
            }
            else
            {
                if (extensionVisual != null)
                    extensionVisual.Activate(Owner, target, false);
                HandleActivate(target, false);
            }

            if (spendTurn)
                LevelController.Current.CompleteTurn();
        }
        public List<Character> GetAllCharactersInRangeFromGrid(Character characterHitCenter)
        {
            List<Character> result = new List<Character>();
            NPC npcHitCenter = characterHitCenter.GetComponent<NPC>();
            if (!TargetInRange(characterHitCenter))
                return result;
            if (multiTarget && npcHitCenter != null)
                foreach (NPC npc in LevelController.Current.enemiesNPC)
                {
                    if (npc.gridPositionFromBottom.y > LevelController.Current.sizeBattlefield.y - 1)
                        continue;
                    Vector2Int relativePos = npc.gridPositionFromBottom - npcHitCenter.gridPositionFromBottom;
                    if (multiTargetSetupList.Exists(e => e.Equals(relativePos)))
                        result.Add(npc.character);
                }
            else
            {
                result.Add(characterHitCenter);
            }
            return result;
        }
        public List<CharacteristicDescription> GetDescription()
        {
            List<CharacteristicDescription> result = new List<CharacteristicDescription>();

            if (limitDistance)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Distance";
                characteristic.valueToCompare = maxDistance;
                characteristic.description = 
                    string.Format("Distance <color>{0}</color>",
                    (characteristic.valueToCompare + 1));
                result.Add(characteristic);
            }
            if (cost.healthCur != 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Health";
                characteristic.valueToCompare = cost.healthCur;
                characteristic.description = 
                    string.Format("Health <color>{0}{1}</color>",
                    cost.healthCur.GetPositiveOrNegativeSign(true),
                    Mathf.Abs((int)characteristic.valueToCompare));
                characteristic.comparisonRule = CharacteristicDescription.ComparisonRule.LessIsBetter;
                result.Add(characteristic);
            }
            if (cost.staminaCur != 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Stamina";
                characteristic.valueToCompare = cost.staminaCur;
                characteristic.description = 
                    string.Format("Stamina <color>{0}{1}</color>",
                    cost.staminaCur.GetPositiveOrNegativeSign(true),
                    Mathf.Abs((int)characteristic.valueToCompare));
                characteristic.comparisonRule = CharacteristicDescription.ComparisonRule.LessIsBetter;
                result.Add(characteristic);
            }
            if (cooldownTime > 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Cooldown";
                characteristic.valueToCompare = cooldownTime;
                characteristic.description =
                    string.Format("Cooldown <color>{0}</color>",
                    characteristic.valueToCompare);
                characteristic.comparisonRule = CharacteristicDescription.ComparisonRule.LessIsBetter;
                result.Add(characteristic);
            }
            result.AddRange(HandleGetDescription().ToArray());

            return result;
        }
        #region Virtual Methods
        private bool TargetInRange(Character character)
        {
            NPC npcHitCenter = character.GetComponent<NPC>();
            if (npcHitCenter != null && limitDistance && npcHitCenter.gridPositionFromBottom.y > maxDistance)
                return false;
            else
                return true;
        }
        protected virtual bool HandleCanBeActivatedOnTarget(Character target)
        {
            return true;
        }
        protected virtual bool HandleCanBeActivated()
        {
            return true;
        }
        protected virtual void HandleActivate(Character target, bool isSecondaryTarget)
        {

        }
        protected virtual List<CharacteristicDescription> HandleGetDescription()
        {
            return new List<CharacteristicDescription>();
        }
        #endregion

        #region Static Methods
        public static bool IsOnCooldown(Ability ability, Character character)
        {
            return character.abilities.Find(
                (x) => { return x == ability && x.RemainCooldown > 0; }) != null;
        }
        #endregion

    }
}