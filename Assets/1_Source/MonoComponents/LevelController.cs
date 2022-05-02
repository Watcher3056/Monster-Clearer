using Cinemachine;
using DG.Tweening;
using Pixeye.Actors;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace TeamAlpha.Source
{
    public partial class LevelController : MonoBehaviour
    {
        #region SAVE_KEYS
        public string SAVE_KEY_PLAYED => guid.id + ".PLAYED";
        public string SAVE_KEY_COMPLETED => guid.id + ".COMPLETED";
        #endregion
        #region DATA_CONTAINERS
        public enum Side { Allie, Enemy }
        public class Statistic
        {
            public int TotalReward => 0;
            public int curTurn;
            public bool playerWon;
            public List<Item> itemsLootInstances = new List<Item>();
        }
        [Serializable, HideLabel]
        public class ItemDropPoolConfiguration
        {
            [ValueDropdown("@((LevelController)$property.SerializationRoot.ValueEntry.WeakSmartValue)._EditorItemLootDropDown")]
            [ValidateInput("@((LevelController)$property.SerializationRoot.ValueEntry.WeakSmartValue)._EditorValidateInputDropPoolConfig($value)",
                "This Tier does not exist!")]
            public int tier;
            [Range(0f, 1f)]
            [InfoBox("Probability is zero. It will never gonna drop!",
                "_EditorShowProbabilityMessageWarning",
                InfoMessageType = InfoMessageType.Warning)]
            public float probability;

            public ItemTierPool Pool => DataGameMain.Default.allItemsTierPools.Find(p => p.tier == this.tier);
            private bool _EditorShowProbabilityMessageWarning => probability == 0f;
        }
        public enum FailReason { None }
        #endregion
        #region RUNTIME
        public static LevelController Current => _current;
        private static LevelController _current;

        public bool LevelPlayed
        {
            get => levelPlayed;
            set
            {
                levelPlayed = value;
                Origin.levelPlayed = value;
                ProcessorSaveLoad.Save(SAVE_KEY_PLAYED, levelPlayed);
            }
        }
        public bool LevelCompleted
        {
            get => levelCompleted;
            set
            {
                levelCompleted = value;
                ProcessorSaveLoad.Save(SAVE_KEY_COMPLETED, value);
            }
        }
        private bool levelCompleted;
        public bool Unlocked
        {
            get
            {
                LevelController lastLevelCompleted = DataGameMain.Default.LastLevelCompleted;
                int lastLevelCompletedIndex = 0;
                if (lastLevelCompleted == null)
                    lastLevelCompletedIndex = -1;
                else
                    lastLevelCompletedIndex = lastLevelCompleted.Index;
                return LevelPlayed || lastLevelCompletedIndex + 1 == this.Index;
            }
        }
        public int Index => DataGameMain.Default.levels.IndexOf(Origin);
        public LevelController Origin => DataGameMain.Default.levels.Find(l => l.guid.id.Equals(guid.id));
        private bool levelPlayed;
        [NonSerialized]
        public Statistic levelStats = new Statistic();
        public static Statistic lastLevelStats;
        [NonSerialized]
        public List<Character> allies = new List<Character>();
        [HideInInspector]
        public List<Character> enemies = new List<Character>();
        [HideInInspector]
        List<Character> allCharacters = new List<Character>();
        [HideInInspector]
        public List<NPC> enemiesNPC = new List<NPC>();
        [NonSerialized]
        public List<Character> turnQueue = new List<Character>();
        public Character ActingNow => actingNow;
        [NonSerialized]
        private Character actingNow;
        public FailReason FailReson { get; private set; }
        public bool HoldTurn
        {
            get => holdTurn || actingNow != null || ActiveTweeners > 0;
            set => holdTurn = value;
        }
        public int ActiveTweeners
        {
            get
            {
                int result = 0;
                foreach (Tween tween in tweeners)
                    if (tween.IsPlaying())
                        result++;
                return result;
            }
        }
        private bool holdTurn;
        private List<Tweener> tweeners = new List<Tweener>();
        #endregion
        #region SETTINGS
        [HideLabel, PropertyOrder(-1000)]
        public ComponentGUID guid;
        [Required, PropertyOrder(-900)]
        public Transform dynamicHolder;
        [PreviewField(150f), Required]
        public Sprite spriteBG;
        [PropertyOrder(-890), MinValue(0)]
        public int totalLootAmount;
        [PropertyOrder(-800)]
        [InfoBox("At least one element required!", InfoMessageType.Error, "_EditorValidateDropPoolConfig")]
        public List<ItemDropPoolConfiguration> lootDropConfiguration;
        [DisableInPrefabAssets, OnValueChanged("_EditorOnSizeBattlefieldValueChanged")]
        public int poolSize = 4;
        [OnValueChanged("_EditorOnSizeBattlefieldValueChanged")]
        public Vector2Int sizeBattlefield = new Vector2Int(5, 4);
        #endregion
        public void Init()
        {
            ProcessorSaveLoad.OnLocalDataUpdated += HandleLocalDataUpdated;

            HandleLocalDataUpdated();
        }
        private void HandleLocalDataUpdated()
        {
            levelPlayed = ProcessorSaveLoad.Load(SAVE_KEY_PLAYED, false);
            levelCompleted = ProcessorSaveLoad.Load(SAVE_KEY_COMPLETED, false);
        }
        public void SetAsCurrent()
        {
            PlayerController.Current.IsRestEnabled = false;
            PanelBattleGrid.Default.Clear();
            PlayerController.Current.ResetResourceStats();
            PanelBattleGrid.Default.imageBG.sprite = spriteBG;
            _current = this;
            FailReson = FailReason.None;

            foreach (Character enemy in enemies)
            {
                Character _enemy = enemy;
                NPC npc = enemy.GetComponent<NPC>();
                enemy.OnDie += () => HandleEnemyDied(_enemy);
                enemiesNPC.Add(npc);
                npc.CooldownLeft = 2;
                PanelBattleGrid.Default.HandleAddEnemy(npc);
            }
            allies.Add(PlayerController.Current.character);

            allCharacters.AddRange(allies.ToArray());
            allCharacters.AddRange(enemies.ToArray());
            foreach (Character character in allCharacters)
            {
                character.ResetAllExpendables();
                character.ResetAllCooldowns();
            }

            PlayerController.Current.character.OnDie += HandlePlayerDied;
        }
        private void HandleEnemyDied(Character enemy)
        {
            NPC npc = enemy.GetComponent<NPC>();
            PanelBattleGrid.Default.HandleNPCDied(npc);
            enemies.Remove(enemy);
            enemiesNPC.Remove(enemiesNPC.Find(npc => npc.character == enemy));
            allCharacters.Remove(enemy);
            turnQueue.Remove(enemy);
            if (!PlayerController.Current.AutoAttackNow)
                UpdateGridPositions();
            PlayerController.Current.statsPlayer.CurExperience += npc.rewardExperience;
        }
        private void HandlePlayerDied()
        {
            allies.Remove(PlayerController.Current.character);
            CompleteLevel();
        }
        private void UpdateGridPositions()
        {
            enemiesNPC
                    .Sort((e1, e2) => e1.gridPositionFromBottom.y.CompareTo(e2.gridPositionFromBottom.y));
            foreach (NPC npc in enemiesNPC)
            {
                if (npc.gridPositionFromBottom.y > 0)
                {
                    NPC npcBelow =
                        enemiesNPC.FindLast(
                            _npc => _npc.gridPositionFromBottom.x == npc.gridPositionFromBottom.x &&
                                    _npc.gridPositionFromBottom.y < npc.gridPositionFromBottom.y);
                    if (npcBelow != null)
                        npc.gridPositionFromBottom.y = npcBelow.gridPositionFromBottom.y + 1;
                    else
                        npc.gridPositionFromBottom.y = 0;
                }
            }
            PanelBattleGrid.Default.UpdateView();
        }
        public void Launch()
        {
            LevelPlayed = true;
            LayerDefault.Default.Playing = true;
            UIManager.Default.CurState = UIManager.State.Battle;
            StartCoroutine(ProcessBattle());
        }
        //public void AddEnemy(NPC npc)
        //{
        //    enemies.Add(npc.character);
        //    PanelBattleGrid.Default.HandleAddEnemy(npc);
        //}
        private IEnumerator ProcessBattle()
        {
            while (enemies.Count > 0 && allies.Count > 0)
            {
                UpdateGridPositions();
                ProcessAbilityCooldown();
                ProcessNPCCooldown();
                ProcessStatsResourcesRestore();

                PanelPlayerToolBarActions.Default.SetActivePlayerControls(true);

                while (PlayerController.Current.CurTarget == null && enemies.Count > 0 && allies.Count > 0)
                    yield return null;

                IEnumerator battleLoop = BattleLoop(false, PlayerController.Current.CurTarget);
                while (battleLoop.MoveNext())
                    yield return battleLoop.Current;

                PlayerController.Current.CurTarget = null;
                levelStats.curTurn++;
            }
            CompleteLevel();
        }
        public IEnumerator BattleLoop(bool simulateOnly, Character playerTarget)
        {
            StatsResources playerStatsResources = new StatsResources();
            playerStatsResources += PlayerController.Current.character.statsResources;
            StatsResources targetStatsResources = new StatsResources();
            if (playerTarget != null)
                targetStatsResources += playerTarget.statsResources;
            bool simulate = simulateOnly;
            while (PlayerController.Current.AutoAttackNow || simulate)
            {
                ResetTurnQueue();
                turnQueue.Remove(playerTarget);

                while (turnQueue.Count > 0)
                {
                    actingNow = turnQueue[0];
                    turnQueue.RemoveAt(0);
                    void NPCDoAttack(NPC npc)
                    {
                        if (simulateOnly)
                        {
                            StatsResources damageToPlayer =
                                npc.CalculateDamageToPlayer();
                            playerStatsResources -=
                                PlayerController.Current.character
                                .GetProcessedDamage(damageToPlayer);
                        }
                        else
                            npc.GetComponent<NPC>().DoAttackPlayer();
                    }

                    if (actingNow == PlayerController.Current.character)
                    {

                        if (simulateOnly)
                        {
                            StatsResources damageToTarget =
                                    PlayerController.Current.AbilityDefaultAttack.GetAllDamage(false);
                            targetStatsResources -= playerTarget.GetProcessedDamage(damageToTarget);
                        }
                        else
                            PlayerController.Current.AbilityDefaultAttack.Activate(PlayerController.Current.CurTarget);

                        bool isTargetAlive = simulateOnly ? targetStatsResources.healthCur > 0f : playerTarget.IsAlive;
                        if (isTargetAlive)
                            NPCDoAttack(playerTarget.GetComponent<NPC>());
                    }
                    else
                    {
                        NPCDoAttack(actingNow.GetComponent<NPC>());
                    }
                }
                actingNow = null;

                if (!simulateOnly)
                {
                    while (HoldTurn)
                        yield return null;
                    tweeners.Clear();
                    yield return new WaitForSeconds(1f / DataGameMain.Default.battleAnimationSpeed);
                }
                else
                {
                    StatsResources totalDamage =
                        PlayerController.Current.character.statsResources - playerStatsResources;
                    PanelBattleGrid.Default.cellPlayer.ShowSimulatedDamage(true, totalDamage);
                }

                simulate =
                    simulateOnly
                    && targetStatsResources.healthCur > 0f
                    && playerStatsResources.healthCur > 0f;
            }
        }
        public void CompleteTurn()
        {
            if (actingNow == PlayerController.Current.character)
                PanelPlayerToolBarActions.Default.SetActivePlayerControls(false);
            actingNow = null;
        }
        public void Update()
        {
            if (!LayerDefault.Default.Playing)
                return;

        }
        public void CheckLevelCompletion()
        {

        }
        public void CompleteLevel()
        {
            PlayerController.Current.character.OnDie -= HandlePlayerDied;
            turnQueue.Clear();
            StopAllCoroutines();
            StartCoroutine(_CompleteLevel());
        }
        public void RegisterTweener(Tweener tweener)
        {
            tweeners.Add(tweener);
        }
        [Sirenix.OdinInspector.Button, PropertyOrder(-700)]
        public void SortDropPoolConfigByProbability()
        {
            lootDropConfiguration.Sort((config1, config2) => config1.probability.CompareTo(config2.probability));
        }
        public List<Item> GetLootPrefabs()
        {
            return GetLootPrefabs(totalLootAmount);
        }
        public List<Item> GetLootPrefabs(int amount)
        {
            SortDropPoolConfigByProbability();

            List<Item> result = new List<Item>();

            for (int i = 0; i < amount; i++)
            {
                foreach (ItemDropPoolConfiguration config in lootDropConfiguration)
                {
                    if (this.RandomYesOrNo(config.probability))
                    {
                        result.Add(config.Pool.itemsPrefabs.Random());
                        break;
                    }
                }
            }

            return result;
        }
        private IEnumerator _CompleteLevel()
        {
            LayerDefault.Default.Playing = false;

            if (enemies.Count == 0)
            {
                levelCompleted = true;
                Origin.LevelCompleted = true;
            }

            float progress =
                (float)(Origin.enemies.Count - enemies.Count) / Origin.enemies.Count;
            List<Item> itemsLootPrefabs = GetLootPrefabs((int)Mathf.Lerp(0, totalLootAmount, progress));
            foreach (Item itemPrefab in itemsLootPrefabs)
            {
                Item itemInstance = itemPrefab.InstantiateFromOrigin(true);
                levelStats.itemsLootInstances.Add(itemInstance);
            }
            levelStats.itemsLootInstances.Reverse();
            foreach (Item itemInstance in levelStats.itemsLootInstances)
                PlayerController.Current.character.AddItemToBackpack(itemInstance, true);

            PanelLevelComplete panelLevelComplete = null;
            if (levelCompleted)
            {
                panelLevelComplete = PanelLevelCompleteWin.Default;

            }
            else
            {
                panelLevelComplete = PanelLevelCompleteFailed.Default;
                PlayerController.Current.RestTimeLeft = DataGameMain.Default.playerRestTime;
            }
            panelLevelComplete.panel.OpenPanel();

            while (panelLevelComplete.panel.CurState == Panel.State.Opened)
                yield return null;

            PlayerController.Current.character.ResetAllCooldowns();
            PlayerController.Current.character.ResetAllExpendables();
            PlayerController.Current.ResetResourceStats();

            PlayerController.Current.IsRestEnabled = true;
            UIManager.Default.CurState = UIManager.State.MainMenu;
        }
        public List<Character> GetTurnQueue()
        {
            List<Character> result = new List<Character>();

            foreach (NPC npc in enemiesNPC)
            {
                if (npc.CanAttack)
                    result.Add(npc.character);
            }
            result.AddRange(allies.ToArray());

            return result;
        }
        private void ResetTurnQueue()
        {
            turnQueue.Clear();
            turnQueue.AddRange(GetTurnQueue());
        }
        private void ProcessAbilityCooldown()
        {
            foreach (Character _character in allCharacters)
            {
                //if (_character == lastTurn)
                //    continue;
                foreach (Ability ability in _character.abilities)
                    if (ability.RemainCooldown > 0)
                        ability.RemainCooldown--;
                _character.RecountStatsAll();
            }
        }
        private void ProcessNPCCooldown()
        {
            foreach (NPC npc in enemiesNPC)
            {
                if (npc.EnemyInRange)
                {
                    if (!npc.firstTurnPassed)
                    {
                        npc.firstTurnPassed = true;
                        continue;
                    }
                    else if (npc.CooldownLeft > 0)
                    {
                        npc.CooldownLeft--;
                    }
                }
            }
        }
        private void ProcessStatsResourcesRestore()
        {
            foreach (Character _character in allCharacters)
            {
                if (_character.statsResources.staminaCur < _character.lvl3StatsResultSum.common.staminaMax)
                    _character.statsResources.staminaCur++;
            }
        }
    }
}