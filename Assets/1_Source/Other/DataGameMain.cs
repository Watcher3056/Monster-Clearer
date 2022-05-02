using Pixeye.Actors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class DataGameMain : Pluggable
    {
        #region SAVE LOAD KEYS
        public const string SAVE_KEY_COINS_AMOUNT = "COINS_AMOUNT";
        public const string SAVE_KEY_VIBRATIONS_IS_ENABLED = "VIBRATIONS_IS_ENABLED";
        #endregion
        #region RUNTIME DATA
        public static DataGameMain Default
        {
            get
            {
                if (!Application.isPlaying && Application.isEditor && _default == null)
                    _default = Resources.Load<DataGameMain>(new SettingsActors().Plugins[0]);
                return _default;
            }
        }
        private static DataGameMain _default;

        public int Coins
        {
            get => coins;
            set
            {
                coins = value;
                ProcessorSaveLoad.Save(SAVE_KEY_COINS_AMOUNT, coins);
            }
        }
        private int coins;
        public bool VibrationsIsEnabled
        {
            get => vibrationsIsEnabled;
            set
            {
                vibrationsIsEnabled = value;
                ProcessorSaveLoad.Save(SAVE_KEY_VIBRATIONS_IS_ENABLED, vibrationsIsEnabled);
            }
        }
        private bool vibrationsIsEnabled;
        public LevelController LastLevelUnlocked => levels.FindLast(l => l.Unlocked);
        public LevelController LastLevelPlayed => levels.FindLast(l => l.LevelPlayed);
        public LevelController LastLevelCompleted => levels.FindLast(l => l.LevelCompleted);

        public static string LayerUIParticlesName => "UIParticles";
        public static int LayerUIParticles { get; private set; }
        #endregion
        #region SETTINGS
        public AudioSimpleData audioOnPanelShow;
        public AudioSimpleData audioOnPanelHide;

        public int skillPointsPerPlayerLevel = 5;
        public int skillPointsPerNPCLevel = 20;
        public int skillPointsNPCFirstLevel = 130;
        public int statsStepPerSkillPoint;
        public int maxCharacterLevel;
        public int firstCharacterLevelExpCost;
        public float levelCostIncrease;
        public float attackDamagePerPoint;
        public float staminaPerPoint;
        //public int magic;
        public float defenceDamageReductionPerPoint;
        public int healthPerLevel;
        public float battleAnimationSpeed = 2f;
        public int staminaRestorePerTurn = 1;
        public int backpackDisposeChestCapacity = 5;
        public float playerRestTime = 10f;

        [Required, AssetsOnly]
        public List<LevelController> levels;
        [Required, AssetsOnly, OnValueChanged("_EditorUpdateItemRarityTypesSortOrder")]
        public List<ItemRarityType> itemRarityTypes;
        [Required, AssetsOnly, OnValueChanged("_EditorUpdateItemRarityTypesSortOrder")]
        public ItemRarityType itemRarityTypeDefault;
        public List<Character> AllCharactersPrefabs
        {
            get
            {
                //#if UNITY_EDITOR
                //                _EditorUpdateData();
                //#endif
                return allCharactersPrefabs;
            }
        }
        [ReadOnly, SerializeField]
        private List<Character> allCharactersPrefabs;
        public List<Item> AllItemsPrefabs
        {
            get
            {
                //#if UNITY_EDITOR
                //                _EditorUpdateData();
                //#endif
                return allItemsPrefabs;
            }
        }
        [ReadOnly, SerializeField, ValidateInput("OnValidateOdin")]
        private List<Item> allItemsPrefabs;
        [ReadOnly, SerializeField]
        public List<ItemTierPool> allItemsTierPools;
        #endregion

#if UNITY_EDITOR
        private bool OnValidateOdin(object obj)
        {
            _EditorUpdateData();
            return true;
        }
        //[OnInspectorInit]
        public void _EditorUpdateData()
        {
            allCharactersPrefabs = GetAllPrefabs<Character>("Assets/2_Content/Characters");
            allItemsPrefabs = GetAllPrefabs<Item>("Assets/2_Content/Items");

            _EditorUpdateItemsTierPools();
            _EditorUpdateItemRarityTypesSortOrder();
            UnityEditor.EditorUtility.SetDirty(this);
            this.Log("Data Was Updated");
        }
        public void _EditorUpdateItemsTierPools()
        {
            allItemsTierPools.Clear();
            List<Item> _items = new List<Item>(allItemsPrefabs.ToArray());
            _items.Sort((item1, item2) => item1.tier.CompareTo(item2.tier));


            int maxTier = _items[_items.Count - 1].tier;

            int curTier = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                Item item = _items[i];
                if (item.tier > curTier)
                {
                    ItemTierPool pool = new ItemTierPool();
                    curTier = item.tier;
                    pool.tier = curTier;
                    allItemsTierPools.Add(pool);
                }
                allItemsTierPools.Find(p => p.tier == item.tier).itemsPrefabs.Add(item);
            }
            UnityEditor.EditorUtility.SetDirty(this);
        }
        public void _EditorUpdateItemRarityTypesSortOrder()
        {
            if (itemRarityTypeDefault != null && 
                !itemRarityTypes.Contains(itemRarityTypeDefault))
            {
                itemRarityTypes.Add(itemRarityTypeDefault);
                itemRarityTypeDefault.colorBG = Color.white;
                UnityEditor.EditorUtility.SetDirty(itemRarityTypeDefault);
            }
            itemRarityTypes.Sort((item1, item2) => item1.probability.CompareTo(item2.probability));
            UnityEditor.EditorUtility.SetDirty(this);
        }
        private List<T> GetAllPrefabs<T>(string assetPath)
        {
            List<T> result = new List<T>();
            string[] guids =
                UnityEditor.AssetDatabase.FindAssets("t:prefab", new string[] { assetPath });
            foreach (string guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                GameObject go = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                T component = go.GetComponent<T>();
                if (component != null)
                    result.Add(component);
            }
            return result;
        }
#endif
        public override void Plug()
        {
            _default = this;
            LayerUIParticles = LayerMask.NameToLayer(LayerUIParticlesName);

            ProcessorSaveLoad.OnLocalDataUpdated += HandleLocalDataUpdated;
        }
        private void HandleLocalDataUpdated()
        {
            coins = ProcessorSaveLoad.Load(SAVE_KEY_COINS_AMOUNT, 0);
            vibrationsIsEnabled = ProcessorSaveLoad.Load(SAVE_KEY_VIBRATIONS_IS_ENABLED, true);
        }
    }
}
