using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using DG.Tweening;

namespace TeamAlpha.Source
{
    public class PanelPlayerToolbarActionsCell : MonoBehaviour
    {
        public enum ContentType { Ability, Item }
        [Required]
        public InteractableController interactable;
        public float animSpeed = 4f;
        [Required]
        public Image imageIcon;
        [Required]
        public Image imageIconBG;
        [Required]
        public GameObject viewLocked;
        //[Required]
        //public GameObject viewEmpty;
        [Required]
        public GameObject viewCooldown;
        [Required]
        public TextMeshProUGUI textAmount;
        [Required]
        public TextMeshProUGUI textCooldown;
        [Required]
        public GameObject viewIndicatorNew;
        public ContentType contentType;
        [FormerlySerializedAs("type"), ShowIf("contentType", ContentType.Item)]
        public Item.Type itemType;

        [NonSerialized]
        public Item linkedItem;
        [NonSerialized]
        public Ability linkedAbility;
        public bool MarkContentAsNew
        {
            get => markContentAsNew;
            set
            {
                markContentAsNew = value;
                UpdateView();
            }
        }
        private bool markContentAsNew;
        public bool Interactable
        {
            set
            {
                interactable.Interactable = value;
                UpdateView();
            }
            get
            {
                bool result = false;
                if (!PanelPlayerToolBarActions.Default.allCells.Contains(this))
                    result = true;
                else if (linkedItem != null)
                    result = linkedItem.CanBeActivated();
                else if (linkedAbility != null)
                    result = linkedAbility.CanBeActivated();

                return result && interactable.Interactable && HasContent;
            }
        }
        public bool HasContent => linkedItem != null || linkedAbility != null;

        private void Awake()
        {
            interactable.OnDragStart += HandleDragStart;
            interactable.OnClick += HandleClick;
            UpdateView();
        }
        private void HandleClick()
        {
            if (PanelBackpack.Default.cells.Contains(this))
                return;
            PanelContextInfo.Default.HideAll();
            PanelContextInfo.Default.ShowInfo(this);
        }
        private void HandleDragStart()
        {
            PlayerController.Current.CellDraggingNow = this;
            SetActiveCellMask(false);
            UIManager.Default.SetHighlightUIElement(interactable.gameObject, true);

            DOTween.To(() => Vector2.zero,
                offset => interactable.Offset = offset,
                PlayerController.Current.offsetOnDrag, 1f / animSpeed);
        }
        public bool ContentCanBeUsedOnTarget(Character target)
        {
            bool result = false;
            if (linkedItem != null)
                result = linkedItem.CanBeActivatedOnTarget(target);
            else if (linkedAbility != null)
                result = linkedAbility.CanBeActivatedOnTarget(target);

            return result;
        }
        public void UseContentOnTarget(Character target)
        {
            if (linkedItem != null)
                linkedItem.Activate(target);
            else if (linkedAbility != null)
                linkedAbility.Activate(target);

            UpdateView();
        }
        public void SetAbility(Ability ability)
        {
            if (contentType != ContentType.Ability)
            {
                this.LogError("Wrong input!");
                return;
            }
            linkedItem = null;
            linkedAbility = ability;
            imageIcon.sprite = ability.spriteIcon;
            //viewEmpty.SetActive(false);
            imageIcon.gameObject.SetActive(true);
            imageIconBG.gameObject.SetActive(true);
            imageIconBG.color = Color.clear;

            UpdateView();
        }
        public void SetItem(Item item)
        {
            if (contentType != ContentType.Item || (item.type != itemType && itemType != Item.Type.Any))
            {
                this.LogError("Wrong input!");
                return;
            }
            linkedAbility = item.ability;
            linkedItem = item;
            imageIcon.sprite = item.spriteIcon;
            //viewEmpty.SetActive(false);
            imageIcon.gameObject.SetActive(true);
            imageIconBG.gameObject.SetActive(true);
            imageIconBG.sprite = item.spriteBG;
            imageIconBG.color = item.RarityType.colorBG;

            UpdateView();
        }
        public void Clear()
        {
            linkedItem = null;
            linkedAbility = null;
            //viewEmpty.SetActive(true);
            imageIcon.gameObject.SetActive(false);
            imageIconBG.gameObject.SetActive(false);

            UpdateView();
        }
        public void UpdateView()
        {
            if (HasContent)
            {
                imageIcon.gameObject.SetActive(true);
                viewLocked.SetActive(!Interactable);
                interactable.Draggable = Interactable;
                SetActiveViewNewIndicator(MarkContentAsNew);
            }
            else
            {
                imageIcon.gameObject.SetActive(false);
                viewLocked.SetActive(false);
                SetActiveViewNewIndicator(false);
            }
            if (linkedItem != null && linkedItem.expendable && linkedItem.amount > 1)
            {
                textAmount.gameObject.SetActive(true);
                textAmount.text = linkedItem.AmountLeft.ToString() + "/" + linkedItem.amount;
            }
            else
                textAmount.gameObject.SetActive(false);
            if (linkedAbility != null && linkedAbility.RemainCooldown > 0)
            {
                viewCooldown.SetActive(true);
                textCooldown.text = linkedAbility.RemainCooldown.ToString();
            }
            else
                viewCooldown.SetActive(false);
        }
        private void SetActiveViewNewIndicator(bool arg)
        {
            viewIndicatorNew.SetActive(arg);
        }
        public void SetActiveCellMask(bool arg)
        {
            imageIcon.maskable = arg;
        }
    }
}
