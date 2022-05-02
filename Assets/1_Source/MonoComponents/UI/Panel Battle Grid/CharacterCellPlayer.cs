using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class CharacterCellPlayer : CharacterCell, IPointerDownHandler, IPointerUpHandler
    {
        [Required]
        public Image imagePlayerHPBar;
        [Required]
        public Image imageOutline;
        [Required]
        public TextMeshProUGUI textPlayerHP;
        [Required]
        public Image imagePlayerRaycastZone;
        [Required]
        public RectTransform arrowPointer;

        public bool Interactable { get; set; }
        public bool Interacting => dragging;
        private bool dragging;
        private Vector3 lastMouseWorldPos;
        private void Awake()
        {
            ProcessorObserver.Default.Add(() => PlayerController.Current.character.statsResources.healthCur,
                health => HandlePlayerHealthChanged(), true);
            arrowPointer.gameObject.SetActive(false);

            SetCharacter(PlayerController.Current.character);
            SetActiveAbilityRangeIndicator(false);
            ShowSimulatedDamage(false);
        }
        private void HandlePlayerHealthChanged()
        {
            imagePlayerHPBar.fillAmount =
                PlayerController.Current.character.statsResources.healthCur /
                PlayerController.Current.character.lvl3StatsResultSum.common.healthMax;
            textPlayerHP.text =
                ((int)PlayerController.Current.character.statsResources.healthCur).ToString() + "/" +
                ((int)PlayerController.Current.character.lvl3StatsResultSum.common.healthMax).ToString();
        }
        public override void SetActiveAbilityRangeIndicator(bool arg)
        {
            base.SetActiveAbilityRangeIndicator(arg);
            if (arg)
            {
                imageOutline.DOFade(1f, 0.15f);
                transform.DOScale(1.1f, 0.15f);
            }
            else
            {
                imageOutline.DOFade(0f, 0.15f);
                transform.DOScale(1f, 0.15f);
            }
        }
        private void Update()
        {
            if (!dragging || !LayerDefault.Default.Playing)
                return;
            lastMouseWorldPos = Input.mousePosition.ScreenToWorldPoint()
                + (Vector3)PlayerController.Current.offsetDefaultAttack;
            UpdateArrowPointerView(lastMouseWorldPos);
            UpdateTarget();
        }
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Interactable)
                return;
            dragging = true;
            arrowPointer.gameObject.SetActive(true);
            arrowPointer.localScale = Vector3.zero;
            arrowPointer.localPosition = Vector3.zero;
            arrowPointer.transform.DOKill();
            arrowPointer.transform.DOScale(1f, 0.25f);
            arrowPointer.transform.localPosition = Vector3.zero;
        }
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!Interactable)
                return;
            dragging = false;
            arrowPointer.transform.DOKill();
            arrowPointer.transform
                .DOMove(lastMouseWorldPos, 0.25f)
                .OnUpdate(() =>
                {
                    UpdateArrowPointerView(lastMouseWorldPos);
                })
                .OnComplete(() =>
                {
                    //arrowPointer.gameObject.SetActive(false);
                    arrowPointer.transform.DOScale(0f, 0.25f);
                });

            CharacterCell cell = GetTarget(lastMouseWorldPos.WorldToScreenPoint());
            if (cell != null)
                PlayerController.Current.CurTarget = cell.LinkedCharacter;

            PanelBattleGrid.Default.ResetAllAbilityRangeIndicators();
        }
        public void ShowSimulatedDamage(bool arg, StatsResources totalDamage = default)
        {
            if (arg)
            {
                StatsResources playerResources = PlayerController.Current.character.statsResources - totalDamage;

                imagePlayerHPBar.fillAmount =
                    playerResources.healthCur /
                    PlayerController.Current.character.lvl3StatsResultSum.common.healthMax;

                textPlayerHP.text =
                    ((int)playerResources.healthCur).ToString() + "/" +
                    ((int)PlayerController.Current.character.lvl3StatsResultSum.common.healthMax).ToString();
            }
            else
            {
                HandlePlayerHealthChanged();
            }
        }
        private CharacterCell GetTarget(Vector3 screenPos)
        {
            CharacterCell result =
                PanelPlayerToolBarActions.Default.GetCharacterCellByScreenPos(screenPos);
            if (result != null)
            {
                if (PlayerController.Current.AbilityDefaultAttack.CanBeActivatedOnTarget(result.LinkedCharacter))
                    return result;
                else
                    result = null;
            }

            return result;
        }
        private void UpdateArrowPointerView(Vector2 lookAt)
        {
            float distance = Vector2.Distance(arrowPointer.position, lookAt);
            arrowPointer.sizeDelta = new Vector2(arrowPointer.sizeDelta.x, distance / arrowPointer.lossyScale.y);

            float angle =
                Vector3.SignedAngle(arrowPointer.parent.transform.up,
                lookAt - (Vector2)arrowPointer.transform.position, Vector3.forward);

            arrowPointer.localEulerAngles = new Vector3(0f, 0f, angle);
        }
        private void UpdateTarget()
        {
            PanelPlayerToolBarActions.Default
                .HandleAbilityPointingByScreenPosition(lastMouseWorldPos.WorldToScreenPoint(),
                PlayerController.Current.AbilityDefaultAttack);

            CharacterCell target = GetTarget(lastMouseWorldPos.WorldToScreenPoint());
            if (target != null)
                SimulateTurn(target);
            else
                ShowSimulatedDamage(false);
        }
        private void SimulateTurn(CharacterCell targetToAttack)
        {
            LevelController.Current.BattleLoop(true, targetToAttack.LinkedCharacter).MoveNext();
        }
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            DebugExtension.DrawPoint(lastMouseWorldPos, Color.red);
        }
    }
}
