using Pixeye.Actors;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static TeamAlpha.Source.Character;

namespace TeamAlpha.Source
{
    public class Item : MonoBehaviour
    {
        public enum Type { Potion, Scroll, MainWeapon, SecondaryItem1, SecondaryItem2, HeadGear, Armor, Shield, Accessories, Any }

        #region SETTINGS
        [HideLabel, Title("Component GUID"), PropertyOrder(-1000f)]
        public ComponentGUID guid;
        [Required, PreviewField(100f), HorizontalGroup(), PropertyOrder(-910f)]
        public Sprite spriteIcon;
        [Required, PreviewField(100f), HorizontalGroup(), PropertyOrder(-900f)]
        public Sprite spriteBG;
        [Required, OnValueChanged("_EditorOnValueChangedItemName")]
        public string itemName;
        [Required]
        public string itemDescription;
        [MinValue(1), InfoBox("$_EditorGetTierInfo"), OnValueChanged("_EditorOnValueChangedTier")]
        [HorizontalGroup("Tier"), InlineButton("_EditorTierDecrement", "-"), InlineButton("_EditorTierIncrement", "+")]
        public int tier = 1;
        public Type type;
        [ShowIf("IsModifyOwnerStats"), SerializeField]
        private StatsModifier statsModifier;
        [AssetsOnly]
        public Ability ability;
        public bool expendable;
        [ShowIf("expendable")]
        public int amount;
        #endregion

        #region RUNTIME
        public StatsModifier StatsModifier => statsModifier * QualityModifier;
        public Character Owner
        {
            get => owner;
            set
            {
                owner = value;
                transform.SetParent(owner?.holderItems);
                if (ability != null)
                    ability.Owner = owner;
            }
        }
        private Character owner;
        public int AmountLeft
        {
            get
            {
                if (this == OriginPrefab)
                    return amount;
                else
                    return amountLeft;
            }
            set
            {
                amountLeft = value;
            }
        }
        private int amountLeft;
        public bool IsModifyOwnerStats =>
            type == Type.Accessories || type == Type.MainWeapon || type == Type.HeadGear ||
            type == Type.Shield || type == Type.Armor || type == Type.Any;
        public Item OriginPrefab =>
            DataGameMain.Default.AllItemsPrefabs.Find(i => i.guid.id.Equals(this.guid.id));
        [ShowInInspector, HideInPrefabAssets, ReadOnly, ShowIf("RarityType")]
        public ItemRarityType RarityType => rarityType;
        private ItemRarityType rarityType;
        [ShowInInspector, HideInPrefabAssets, ReadOnly]
        public float QualityModifier => qualityModifier;
        private float qualityModifier = 1f;
        #endregion

#if UNITY_EDITOR
        private void _EditorOnValueChangedItemName() => gameObject.name = itemName;
        private string _EditorGetTierInfo()
        {
            string result = "Currently highest tier(not including this item) is: ";

            ItemTierPool highestTierPool = DataGameMain.Default.allItemsTierPools
            .FindLast(t => (t.itemsPrefabs.Contains(OriginPrefab) && t.itemsPrefabs.Count > 1) ||
                            !t.itemsPrefabs.Contains(OriginPrefab));
            if (highestTierPool == null)
                result += 1;
            else
                result += highestTierPool.tier;
            return result;
        }
        private void _EditorOnValueChangedTier() => DataGameMain.Default._EditorUpdateData();
        //[Sirenix.OdinInspector.Button("+")]
        private void _EditorTierIncrement()
        {
            tier++;
        }
        //[Sirenix.OdinInspector.Button("-")]
        private void _EditorTierDecrement()
        {
            tier--;
        }
#endif

        private void Start()
        {
            HandleStart();
        }
        protected virtual void HandleStart()
        {

        }
        public Item InstantiateFromOrigin(bool assignRandomRarity)
        {
            Item itemInstance = Instantiate(OriginPrefab).GetComponent<Item>();
            if (ability != null)
            {
                Ability abilityInstance =
                    Instantiate(itemInstance.ability.gameObject, itemInstance.transform).GetComponent<Ability>();
                abilityInstance.transform.localPosition = Vector3.zero;
                itemInstance.ability = abilityInstance;
            }
            itemInstance.ResetAmount();
            if (assignRandomRarity)
                itemInstance.AssignRarityTypeRandom();
            else
                itemInstance.AssignRarityType(DataGameMain.Default.itemRarityTypeDefault);
            return itemInstance;
        }
        public void AssignRarityTypeRandom()
        {
            foreach (ItemRarityType rarityType in DataGameMain.Default.itemRarityTypes)
            {
                if (this.RandomYesOrNo(rarityType.probability))
                {
                    AssignRarityType(rarityType);
                    return;
                }
            }
        }
        public void AssignRarityType(ItemRarityType rarityType)
        {
            this.rarityType = rarityType;
            qualityModifier = UnityEngine.Random.Range(rarityType.statsMultiplier.x, rarityType.statsMultiplier.y);
            if (ability != null)
                ability.Power = qualityModifier;
        }
        public void ResetAmount()
        {
            if (!expendable)
                return;
            AmountLeft = amount;
        }
        public void OnRecountOwnerStats()
        {
            if (!IsModifyOwnerStats)
                return;
            Owner.lvl2StatsItems +=
                Owner.lvl0StatsCharacter.ModifyExcludeOrigin(StatsModifier);

            HandleRecountTargetStats();
        }
        protected virtual void HandleRecountTargetStats()
        {

        }

        public bool CanBeActivatedOnTarget(Character target)
        {
            return
                (CanBeActivated() && ability.CanBeActivatedOnTarget(target) &&
                HandleCanBeActivatedOnTarget(target));
        }
        public bool CanBeActivated()
        {
            bool isExpended = expendable && AmountLeft == 0;
            bool abilityCanBeActivated = ability != null && ability.CanBeActivated();
            return
                (!isExpended && abilityCanBeActivated &&
                HandleCanBeActivated());
        }
        public void Activate(Character target)
        {
            if (expendable)
                AmountLeft--;
            ability.Activate(target);

            HandleActivate(target);
        }
        public List<CharacteristicDescription> GetDescription()
        {
            List<CharacteristicDescription> result = new List<CharacteristicDescription>();

            string FormatName(string input)
            {
                string result = new string(input.ToArray());
                //Make first character in upper case
                StringBuilder sb = new StringBuilder(result);
                sb[0] = char.ToUpper(result[0]);
                result = sb.ToString();
                for (int i = 0; i < result.Length; i++)
                {
                    //Add spaces between words
                    if (char.IsUpper(result[i]))
                        result.Insert(i, " ");
                }
                return result;
            }
            if (IsModifyOwnerStats)
            {
                #region GET_FIELDS_WITH_REFLECTION
                List<FieldInfoContainer> fieldsIncrement = new List<FieldInfoContainer>();
                List<FieldInfoContainer> fieldsMultiplier = new List<FieldInfoContainer>();
                List<FieldInfoContainer> fields = new List<FieldInfoContainer>();

                Func<FieldInfoContainer, bool> filterIncrementStats = f => Convert.ToSingle(f.Value) == 0f;
                Func<FieldInfoContainer, bool> filterMultiplyStats = f => Convert.ToSingle(f.Value) == 1f;

                fieldsIncrement.AddRange(StatsModifier.increment.main.GetFields(filterIncrementStats));
                fieldsIncrement.AddRange(StatsModifier.increment.common.GetFields(filterIncrementStats));
                fieldsMultiplier.AddRange(StatsModifier.multiply.main.GetFields(filterMultiplyStats));
                fieldsMultiplier.AddRange(StatsModifier.multiply.common.GetFields(filterMultiplyStats));
                fields.AddRange(fieldsIncrement.ToArray());
                fields.AddRange(fieldsMultiplier.ToArray());
                #endregion

                foreach (FieldInfoContainer field in fields)
                {
                    CharacteristicDescription characteristic = new CharacteristicDescription();
                    characteristic.valueToCompare = Convert.ToSingle(field.Value);

                    characteristic.nameToCompare = FormatName(field.fieldInfo.Name);

                    if (fieldsIncrement.Contains(field))
                        characteristic.description = string.Format("{0} <color>{1}{2}</color>",
                            characteristic.nameToCompare,
                            characteristic.valueToCompare.GetPositiveOrNegativeSign(),
                            (int)characteristic.valueToCompare);
                    else
                    {
                        characteristic.valueToCompare -= 1f;
                        characteristic.description = string.Format("{0} <color>{1}{2:P0}</color>",
                            characteristic.nameToCompare,
                            characteristic.valueToCompare.GetPositiveOrNegativeSign(),
                            characteristic.valueToCompare);
                    }

                    result.Add(characteristic);
                }
            }

            return result;
        }
        private void OnDestroy()
        {
            owner = null;
        }
        #region Virtual Methods
        protected virtual bool HandleCanBeActivatedOnTarget(Character target)
        {
            return true;
        }
        protected virtual bool HandleCanBeActivated()
        {
            return true;
        }
        protected virtual void HandleActivate(Character target)
        {

        }
        #endregion
    }
}