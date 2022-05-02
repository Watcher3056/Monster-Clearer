using Animancer;
using Pixeye.Actors;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TeamAlpha.Source
{

    public partial class LayerDefault : Layer<LayerDefault>
    {
        public static LayerDefault Default { get; private set; }

        [Required]
        public Transform levelHolder;
        public event Action OnAnimationGlobalSpeedChanged = () => { };

        public static float TimeScale { get; set; }
        public static float DeltaTime => UnityEngine.Time.deltaTime * TimeScale;
        public static float DeltaTimeFixed => UnityEngine.Time.fixedDeltaTime * TimeScale;
        public bool Playing
        {
            get => playing && !Paused &&
                PanelLoadingScreen.Default?.panel.CurState != Panel.State.Opened &&
                PanelBattleGrid.Default.panel.CurState == Panel.State.Opened;
            set
            {
                playing = value;
            }
        }
        public bool Paused { get; set; }
        public LevelController CurLevel => DataGameMain.Default.levels[curLevelIndex];
        public LevelController PrevLevel => DataGameMain.Default.levels[prevLevelIndex];
        public float AnimSpeedGlobal
        {
            get => animSpeedGlobal;
            set
            {
                foreach (AnimancerComponent animancer in animancers)
                    animancer.Playable.Speed = value;
                animSpeedGlobal = value;
                OnAnimationGlobalSpeedChanged();
            }
        }
        private float animSpeedGlobal;
        private bool playerWon;
        private bool playing;
        private int curLevelIndex;
        private int prevLevelIndex;
        private List<AnimancerComponent> animancers = new List<AnimancerComponent>();
        private bool firstStartPassed;

        public LayerDefault() => Default = this;
        protected override void Setup()
        {
            Time.scale = 1f;
            TimeScale = 1f;

#if UNITY_EDITOR
            DataGameMain.Default._EditorUpdateData();
#endif
            //Add<ProcessorDebug>();
            //Add<ProcessorDeferredOperation>();
            //Add<ProcessorSaveLoad>();
            //Add<ProcessorTweens>();
            //Add<ProcessorSoundPool>();

            UIManager.Default.CurState = UIManager.State.MainMenu;

            foreach (LevelController level in DataGameMain.Default.levels)
                level.Init();

            curLevelIndex = GetLastLevelToPlayOrFinal().Index;
            //Continue();
        }
        public LevelController GetLastLevelToPlayOrFinal()
        {
            if (DataGameMain.Default.levels.Count(l => l.Unlocked) == DataGameMain.Default.levels.Count)
                return DataGameMain.Default.levels.Last();
            else
                return GetLastLevelToPlay();
        }
        public LevelController GetLastLevelToPlay()
        {
            int index = 0;

            index = DataGameMain.Default.levels.IndexOf(DataGameMain.Default.levels.FindLast(l => l.Unlocked && !l.LevelCompleted));
            if (index < 0 && curLevelIndex == DataGameMain.Default.levels.Count)
                index = 0;

            if (index < 0)
            {
                index = curLevelIndex + 1;
                if (index >= DataGameMain.Default.levels.FindAll(l => l.Unlocked).Count)
                    index = 0;
            }

            return DataGameMain.Default.levels[index];
        }
        public void Continue()
        {
            prevLevelIndex = curLevelIndex;
            if (LevelController.Current != null && LevelController.Current.LevelCompleted)
            {
                curLevelIndex = DataGameMain.Default.levels.IndexOf(GetLastLevelToPlay());
                //Analytics.Events.LevelStarted(curLevelIndex + 1);
            }
#if UNITY_EDITOR
            if (!firstStartPassed)
            {
                //if (_EditorChooseLevel)
                //    curLevelIndex = _EditorCurLevelIndex;
            }
#endif
            //else
            //    Analytics.Events.LevelRestart(curLevelIndex + 1);
            UpdateLevel();
            AnimSpeedGlobal = 1f;
        }
        public void LaunchLevel(LevelController level)
        {
            prevLevelIndex = curLevelIndex;
            curLevelIndex = DataGameMain.Default.levels.IndexOf(level);
            UpdateLevel();
        }
        private void UpdateLevel()
        {
            EnableSelectedLevel(curLevelIndex);

            animancers = new List<AnimancerComponent>(FindObjectsOfType<AnimancerComponent>());
        }
        private void EnableSelectedLevel(int levelIndex)
        {
            curLevelIndex = levelIndex;
            while (levelHolder.childCount != 0)
            {
                GameObject go = levelHolder.GetChild(0).gameObject;
                go.transform.SetParent(null);
                DestroyImmediate(go);
            }
            LevelController curLevel = DataGameMain.Default.levels[levelIndex];
            Instantiate(curLevel.gameObject, levelHolder);

            if (Application.isPlaying)
            {
                if (!firstStartPassed)
                {
                    firstStartPassed = true;
                    ProcessorDeferredOperation.Default.Add(() =>
                    {
                        levelHolder.GetComponentInChildren<LevelController>().SetAsCurrent();
                        LevelController.Current.Launch();
                    }, true, 1);
                }
                else
                {
                    levelHolder.GetComponentInChildren<LevelController>().SetAsCurrent();

                    LevelController.Current.Launch();
                }
            }
        }
    }
}
