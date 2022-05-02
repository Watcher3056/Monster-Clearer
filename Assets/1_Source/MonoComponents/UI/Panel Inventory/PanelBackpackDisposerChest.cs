using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelBackpackDisposerChest : MonoBehaviour
    {
        [Required]
        public CanvasGroup viewHoverItem;
        [Required]
        public CanvasGroup viewNormal;
        [Required]
        public CanvasGroup viewFull;
        [Required]
        public Button button;
        [Required]
        public Image imageProgressBar;
        [Required]
        public RectTransform holderProgressBarSeparators;
        [Required]
        public Sprite spriteSeparator;
        public ParticlesList particlesOnDispose;
        public ParticlesList particlesOnFull;
        public float animSpeed = 6f;

        [OnValueChanged("UpdateViewMaxCapacity"), 
            OnInspectorGUI("UpdateViewMaxCapacity"), 
            ShowInInspector, 
            ReadOnly, HideInPrefabAssets]
        public int MaxCapacity => DataGameMain.Default.backpackDisposeChestCapacity;
        public float FillProgress =>
            Mathf.Clamp01((float)PlayerController.Current.BackpackDisposeChestAmount / MaxCapacity);
        public bool Hover { get; private set; }

        private void Awake()
        {
            ProcessorObserver.Default.Add(() => FillProgress, fillProgress =>
            {
                UpdateViewFill();
                DoHoverOut();
            }, true);
            button.onClick.AddListener(HandleClick);

            UpdateViewMaxCapacity();
        }
        private void Update()
        {
            if (PlayerController.Current.CellDraggingNow == null)
                return;

            PanelBackpackDisposerChest _disposerChest =
                UIManager.Default.Raycast<PanelBackpackDisposerChest>(PlayerController.Current.GetPointerScreenPosition());
            if (_disposerChest != null && _disposerChest == this)
            {
                HandleItemHoverIn();
            }
            else if (Hover)
            {
                HandleItemHoverOut();
            }
        }
        private void UpdateViewMaxCapacity()
        {
            holderProgressBarSeparators.DestroyAllChilds();

            float offsetPerStep =
                imageProgressBar.GetComponent<RectTransform>().rect.width / MaxCapacity;
            for (int i = 1; i < MaxCapacity; i++)
            {
                Image separator = new GameObject("Separator", typeof(Image)).GetComponent<Image>();
                separator.sprite = spriteSeparator;
                separator.preserveAspect = true;
                separator.transform.SetParent(holderProgressBarSeparators);
                separator.transform.localScale = Vector3.one / 2f;
                RectTransform rectTransform = separator.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(0f, 0.5f);
                Vector3 anchorPos = new Vector3(offsetPerStep * i, 0f, 0f);
                rectTransform.anchoredPosition3D = anchorPos;
            }
        }
        private void UpdateView()
        {

        }
        public void UpdateViewFill()
        {
            imageProgressBar.fillAmount = FillProgress;

            if (FillProgress == 1f)
            {
                button.interactable = true;
                viewFull.DOFade(1f, 0.25f);
                particlesOnFull.Restart();
                //viewNormal.transform.DOScale(0f, 1f / animSpeed);
            }
            else
            {
                button.interactable = false;
                viewFull.DOFade(0f, 0.25f);
                particlesOnFull.Stop();
                //viewNormal.transform.DOScale(1f, 1f / animSpeed);
            }
        }
        public void HandleItemHoverIn()
        {
            if (Hover)
                return;
            DoHoverIn();
        }
        public void HandleItemHoverOut()
        {
            if (!Hover)
                return;
            DoHoverOut();
        }
        private void DoHoverIn()
        {
            Hover = true;
            viewHoverItem.DOFade(1f, 1f / animSpeed);
        }
        private void DoHoverOut()
        {
            Hover = false;
            viewHoverItem.DOFade(0f, 1f / animSpeed);
        }
        public void HandleItemDispose(PanelPlayerToolbarActionsCell cell)
        {
            cell.interactable.transform.DOScale(0f, 0.15f).OnComplete(() =>
            {
                particlesOnDispose.Restart();
                PlayerController.Current.character.RemoveItem(cell.linkedItem, true);
                PlayerController.Current.BackpackDisposeChestAmount++;
                DoHoverOut();
                UpdateViewFill();
            });
        }
        private void HandleClick()
        {
            PanelChestReward.Default.ShowReward();
        }
    }
}
