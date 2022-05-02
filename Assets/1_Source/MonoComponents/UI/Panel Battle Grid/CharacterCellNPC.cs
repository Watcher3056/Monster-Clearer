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
    public class CharacterCellNPC : CharacterCell
    {
        [Required]
        public GameObject viewDead;
        [Required]
        public Image imageIcon;
        [Required]
        public Image imageDamageProgressBar;
        [Required]
        public Image imageIndicatorStatus;
        [Required]
        public Image imageAbilityRangeIndicator;

        [ShowInInspector, HideInEditorMode, ReadOnly]
        public NPC LinkedNPC { get; private set; }

        public override void SetCharacter(Character character)
        {
            base.SetCharacter(character);

            LinkedNPC = character.GetComponent<NPC>();
            imageIcon.sprite = character.spriteIcon;
            viewDead.SetActive(false);
            imageAbilityRangeIndicator.gameObject.SetActive(false);
        }
        public override void HandleDamageReceived()
        {
            base.HandleDamageReceived();

            //cell.transform.DOKill();
            transform.DOPunchRotation(Vector3.forward * 45f, 0.25f, 5);
        }
        public override void ClearCharacter()
        {
            base.ClearCharacter();

            LinkedNPC = null;
        }
        public override void SetActiveAbilityRangeIndicator(bool arg)
        {
            base.SetActiveAbilityRangeIndicator(arg);

            imageAbilityRangeIndicator.gameObject.SetActive(arg);
        }
        public override void UpdateView(float animSpeed)
        {
            base.UpdateView(animSpeed);

            //cell.imageDamageProgressBar.DOKill();
            float damageNormalized = Mathf.Lerp(1f, 0f,
                LinkedCharacter.statsResources.healthCur /
                LinkedCharacter.lvl3StatsResultSum.common.healthMax);
            imageDamageProgressBar.DOFillAmount(damageNormalized, 0.5f / animSpeed);
        }
    }
}
