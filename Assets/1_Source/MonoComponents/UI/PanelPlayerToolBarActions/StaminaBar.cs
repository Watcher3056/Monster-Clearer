using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class StaminaBar : MonoBehaviour
    {
        [Required]
        public TextMeshProUGUI textProgress;
        [Required]
        public Image imageProgressBar;

        private void Awake()
        {
            ProcessorObserver.Default.Add(() => PlayerController.Current.character.statsResources.staminaCur, 
                stamina => HandleStaminaChanged(), true);
        }
        private void HandleStaminaChanged()
        {
            float fillAmount =
                PlayerController.Current.character.statsResources.staminaCur /
                PlayerController.Current.character.lvl3StatsResultSum.common.staminaMax;

            textProgress.text = 
                PlayerController.Current.character.statsResources.staminaCur.ToString() + "/" +
                PlayerController.Current.character.lvl3StatsResultSum.common.staminaMax;
            imageProgressBar.fillAmount = fillAmount;
        }
    }
}
