using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class Character : MonoBehaviour
    {
        [Serializable]
        public class EquipmentSlot
        {
            public Item.Type type;
            public Item equipedItem;

            public EquipmentSlot(Item.Type type) => this.type = type;

            public static readonly EquipmentSlot[] standartEquipmentSlotsCached = GetStandartEquimpentSlots().ToArray();
            public static List<EquipmentSlot> GetStandartEquimpentSlots()
            {
                return new List<EquipmentSlot>
                {
                    new EquipmentSlot(Item.Type.Accessories), new EquipmentSlot(Item.Type.Accessories),
                    new EquipmentSlot(Item.Type.Armor),
                    new EquipmentSlot(Item.Type.HeadGear),
                    new EquipmentSlot(Item.Type.MainWeapon),
                    new EquipmentSlot(Item.Type.Potion), new EquipmentSlot(Item.Type.Potion),
                    new EquipmentSlot(Item.Type.SecondaryItem1), new EquipmentSlot(Item.Type.SecondaryItem2),
                    new EquipmentSlot(Item.Type.Shield)
                };
            }
        }
        public enum LVLStats
        {
            lvl0_Character,
            lvl1_StatusEffects,
            lvl2_Items,
            lvl3_ResultSum
        }
        public enum Team { Player, NPC }

        [HideLabel]
        public ComponentGUID guid;
        [Required, PreviewField(100f)]
        public Sprite spriteIcon;
        [Required]
        public string characterName;
        [OnValueChanged("_EditorUpdateView", true)]
        public Stats lvl0StatsCharacter = new Stats();
        [NonSerialized]
        public Stats lvl1StatsEffects = new Stats();
        [NonSerialized]
        public Stats lvl2StatsItems = new Stats();
        [NonSerialized, ShowInInspector, ReadOnly]
        public Stats lvl3StatsResultSum = new Stats();
        [NonSerialized]
        public StatsResources statsResources = new StatsResources();
        [Required, AssetsOnly, InspectorName("Abilities"), SerializeField]
        private List<Ability> abilitiesToAdd = new List<Ability>();
        [Required, AssetsOnly, InspectorName("Items"), SerializeField]
        private List<Item> itemsToAdd = new List<Item>();
        [NonSerialized]
        public List<Ability> abilities = new List<Ability>();
        [NonSerialized]
        public List<StatusEffect> statusEffects = new List<StatusEffect>();
        [NonSerialized]
        public List<Item> backpack = new List<Item>();
        public bool IsAlive => statsResources.healthCur > 0;
        public Character Origin => DataGameMain.Default.AllCharactersPrefabs.Find(ch => ch.guid.id.Equals(guid.id));
        public Team CurTeam => LevelController.Current.allies.Contains(this) ? Team.Player : Team.NPC;


        [NonSerialized]
        public Transform holderAbilities;
        [NonSerialized]
        public Transform holderItems;
        [NonSerialized]
        public Transform holderStatusEffects;

        public event Action OnDie = () => { };
        public event Action OnAbilitiesChanged = () => { };
        public event Action OnBackpackChanged = () => { };
        public event Action OnItemEquipedOrUnequiped = () => { };

        [HideInInspector, NonSerialized]
        public List<EquipmentSlot> equipmentSlots = EquipmentSlot.GetStandartEquimpentSlots();

        private void Awake()
        {
            holderAbilities = new GameObject("Abilities").transform;
            holderAbilities.SetParent(transform);
            holderItems = new GameObject("Items").transform;
            holderItems.SetParent(transform);
            holderStatusEffects = new GameObject("Status Effects").transform;
            holderStatusEffects.SetParent(transform);

            foreach (Ability ability in abilitiesToAdd)
                AddAbility(ability);
            foreach (Item item in itemsToAdd)
            {
                EquipItem(item.InstantiateFromOrigin(true));
            }

            RecountStatsAll();
            statsResources.healthCur = lvl3StatsResultSum.common.healthMax;
            statsResources.staminaCur = lvl3StatsResultSum.common.staminaMax;
        }
        private void Update()
        {

        }

#if UNITY_EDITOR
        [OnInspectorInit]
        private void _EditorUpdateView()
        {
            RecountStatsAll();
        }
#endif
        public void AddAbility(Ability prefabAbility)
        {
            Ability ability = Instantiate(prefabAbility, holderAbilities).GetComponent<Ability>();
            ability.Owner = this;
            abilities.Add(ability);
            OnAbilitiesChanged.Invoke();
        }
        public void RecountStatsAll()
        {
            RecountStats(LVLStats.lvl0_Character);
            RecountStats(LVLStats.lvl1_StatusEffects);
            RecountStats(LVLStats.lvl2_Items);
            RecountStats(LVLStats.lvl3_ResultSum);
        }
        public void RecountStats(LVLStats lvl)
        {
            switch (lvl)
            {
                case LVLStats.lvl0_Character:
                    break;
                case LVLStats.lvl1_StatusEffects:
                    lvl1StatsEffects = new Stats();

                    //Sort by priority
                    statusEffects.Sort(
                        (x1, x2) => x1.priority.CompareTo(x2.priority)
                        );
                    for (int i = 0; i < statusEffects.Count; i++)
                    {
                        StatusEffect statusEffect = statusEffects[i];

                        statusEffect.OnRecountTargetStats();
                    }
                    break;
                case LVLStats.lvl2_Items:
                    lvl2StatsItems = new Stats();

                    for (int i = 0; i < equipmentSlots.Count; i++)
                    {
                        Item item = equipmentSlots[i].equipedItem;
                        if (item == null)
                            continue;

                        item.OnRecountOwnerStats();
                    }
                    break;
                case LVLStats.lvl3_ResultSum:

                    StatsResources statsResourcesBefore = new StatsResources();
                    if (Application.isPlaying)
                    {
                        statsResourcesBefore += statsResources;
                    }
                    Stats statsBefore = new Stats();
                    statsBefore += lvl3StatsResultSum;

                    lvl3StatsResultSum = lvl0StatsCharacter + lvl1StatsEffects + lvl2StatsItems;
                    lvl3StatsResultSum.common += lvl0StatsCharacter.main.GetCommonStatsByConversion();
                    lvl3StatsResultSum.common += lvl1StatsEffects.main.GetCommonStatsByConversion();
                    lvl3StatsResultSum.common += lvl2StatsItems.main.GetCommonStatsByConversion();

                    if (Application.isPlaying)
                    {
                        if (lvl3StatsResultSum.common.healthMax == 0)
                        {
                            lvl3StatsResultSum.common.healthMax = 1;
                            statsResources.healthCur = 1;
                        }
                        else if (statsResources.healthCur > lvl3StatsResultSum.common.healthMax)
                        {
                            statsResources.healthCur = lvl3StatsResultSum.common.healthMax;
                        }
                        else if (statsBefore.common.healthMax > 0)
                        {
                            statsResources.healthCur =
                                Mathf.RoundToInt((statsResourcesBefore.healthCur / statsBefore.common.healthMax) *
                                                (float)lvl3StatsResultSum.common.healthMax);
                        }

                        if (lvl3StatsResultSum.common.staminaMax == 0)
                        {
                            lvl3StatsResultSum.common.staminaMax = 1;
                            statsResources.staminaCur = 1;
                        }
                        else if (statsResources.staminaCur > lvl3StatsResultSum.common.staminaMax)
                        {
                            statsResources.staminaCur = lvl3StatsResultSum.common.staminaMax;
                        }
                        else if (statsBefore.common.staminaMax > 0)
                        {
                            statsResources.staminaCur =
                                Mathf.RoundToInt((statsResourcesBefore.staminaCur / statsBefore.common.staminaMax) *
                                                (float)lvl3StatsResultSum.common.staminaMax);
                        }

                        if (statsResources.staminaCur > lvl3StatsResultSum.common.staminaMax)
                        {
                            statsResources.staminaCur = lvl3StatsResultSum.common.staminaMax;
                        }
                        else if (statsBefore.common.staminaMax > 0)
                        {
                            statsResources.staminaCur =
                                Mathf.RoundToInt((statsResourcesBefore.staminaCur / statsBefore.common.staminaMax) *
                                                (float)lvl3StatsResultSum.common.staminaMax);
                        }
                    }

                    break;
            }

        }

        public void EquipItem(Item itemInstance)
        {
            EquipmentSlot slot = equipmentSlots.Find(s => s.type == itemInstance.type && s.equipedItem == null);
            if (slot == null)
                slot = equipmentSlots.Find(s => s.type == itemInstance.type);

            EquipItem(itemInstance, slot);
        }
        public void EquipItem(Item itemInstance, EquipmentSlot slot)
        {
            if (slot.equipedItem != null)
                UnEquipItem(slot.equipedItem);
            if (equipmentSlots.Exists(s => s.equipedItem == itemInstance))
                UnEquipItem(itemInstance);
            if (backpack.Contains(itemInstance))
                RemoveItemFromBackpack(itemInstance);
            slot.equipedItem = itemInstance;
            itemInstance.Owner = this;

            RecountStatsAll();

            OnItemEquipedOrUnequiped.Invoke();
        }
        public void UnEquipItem(Item itemInstance)
        {
            EquipmentSlot slot = equipmentSlots.Find(s => s.equipedItem == itemInstance);
            slot.equipedItem = null;
            AddItemToBackpack(itemInstance, true);

            RecountStatsAll();

            OnItemEquipedOrUnequiped.Invoke();
        }
        public void AddItemToBackpack(Item itemInstance, bool appendToStart)
        {
            itemInstance.Owner = this;
            if (appendToStart)
                backpack.Insert(0, itemInstance);
            else
                backpack.Add(itemInstance);
            OnBackpackChanged.Invoke();
        }
        public void RemoveItem(Item itemInstance, bool destroyItem = false)
        {
            if (equipmentSlots.Exists(s => s.equipedItem == itemInstance))
                UnEquipItem(itemInstance);
            if (backpack.Contains(itemInstance))
                RemoveItemFromBackpack(itemInstance, destroyItem);
        }
        public void RemoveItemFromBackpack(Item itemInstance, bool destroyItem = false)
        {
            backpack.Remove(itemInstance);
            if (destroyItem)
                Destroy(itemInstance.gameObject);
            OnBackpackChanged.Invoke();
        }
        public void ProcessHealing(StatsResources amount)
        {
            statsResources += amount;
            RecountStats(LVLStats.lvl3_ResultSum);
        }
        public void ProcessDamage(StatsResources amount)
        {
            ProcessDamage(null, amount);
        }
        public void ProcessDamage(Character source, StatsResources amount)
        {
            statsResources -= GetProcessedDamage(amount);
            if (statsResources.healthCur <= 0f)
                Kill();
            RecountStats(LVLStats.lvl3_ResultSum);
        }
        public StatsResources GetProcessedDamage(StatsResources damageInput)
        {
            StatsResources result = new StatsResources() + damageInput;
            result.healthCur = 
                Mathf.Clamp(result.healthCur - lvl3StatsResultSum.common.damageReduction, 0f, result.healthCur);

            return result;
        }
        public List<Item> GetAllItems()
        {
            List<Item> result = new List<Item>();
            result.AddRange(backpack);

            foreach (EquipmentSlot part in equipmentSlots)
                if (part.equipedItem != null)
                    result.Add(part.equipedItem);

            return result;
        }
        public bool Contains(Item item) => item.Owner == this;
        public void ResetAllExpendables()
        {
            List<Item> expendables = GetAllItems().FindAll(item => item.expendable);

            foreach (Item item in expendables)
                item.ResetAmount();
            OnBackpackChanged.Invoke();
        }
        public void ResetAllCooldowns()
        {
            foreach (Ability ability in abilities)
                ability.RemainCooldown = 0;
        }
        private void Kill()
        {
            statsResources.healthCur = 0f;
            OnDie.Invoke();
        }
    }
}
