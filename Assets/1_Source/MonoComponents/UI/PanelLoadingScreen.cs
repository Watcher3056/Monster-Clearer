using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelLoadingScreen : MonoBehaviour
    {
        public static PanelLoadingScreen Default { private set; get; }

        [Required]
        public Panel panel;
        [Required]
        public Slider slider;

        private bool loadingScreenOpened;
        // Use this for initialization

        public PanelLoadingScreen() => Default = this;
        private void Start()
        {
            Load(() => SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive));
        }
        public void Load(Action onComplete = default, params Func<AsyncOperation>[] loadActions)
        {
            StartCoroutine(_Load(onComplete, loadActions));
        }
        private IEnumerator _Load(Action onComplete = default, params Func<AsyncOperation>[] loadActions)
        {
            ToggleLoadingScreen(true);
            while (!loadingScreenOpened)
                yield return null;

            float curProgress = 0f;
            float finalProgress = loadActions.Length;

            foreach (Func<AsyncOperation> loadAction in loadActions)
            {
                AsyncOperation asyncOperation = loadAction.Invoke();
                bool unusedAssetsUnloaded = false;
            unloadUnusedAssets:
                while (asyncOperation.progress < 1f)
                {
                    yield return null;
                    SetProgress((curProgress + asyncOperation.progress) / finalProgress);
                }
                if (!unusedAssetsUnloaded)
                {
                    GC.Collect();
                    asyncOperation = Resources.UnloadUnusedAssets();
                    unusedAssetsUnloaded = true;
                    goto unloadUnusedAssets;
                    curProgress += 1f;
                }
            }
            ToggleLoadingScreen(false);
            if (onComplete != null)
                onComplete.Invoke();
        }
        public void ToggleLoadingScreen(bool arg, Action onComplete = default)
        {
            if (arg)
                panel.OpenPanel();
            else
            {
                ProcessorDeferredOperation.Default.Add(panel.ClosePanel, true, false, 0.5f);
            }
            ProcessorDeferredOperation.Default.Add(() =>
            {
                panel.animancer.States.Current.Events.OnEnd = () =>
                {
                    if (onComplete != null)
                        onComplete.Invoke();
                    loadingScreenOpened = arg;
                };
            }, true);
        }
        public void SetProgress(float progress)
        {
            slider.value = progress;
        }
    }
}