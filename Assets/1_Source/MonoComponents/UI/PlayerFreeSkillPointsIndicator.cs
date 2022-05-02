using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class PlayerFreeSkillPointsIndicator : MonoBehaviour
    {
        [Required]
        public TextMeshProUGUI textPlayerLevel;
        public string prefix = "Skill Points: ";

        private void Awake()
        {
            ProcessorObserver.Default.Add(
                () => PlayerController.Current.statsPlayer.skillPointsLeft, curLevel => UpdateView(), true);
        }
        private void UpdateView()
        {
            textPlayerLevel.text = prefix +
                PlayerController.Current.statsPlayer.skillPointsLeft.ToString();
        }
    }
}
