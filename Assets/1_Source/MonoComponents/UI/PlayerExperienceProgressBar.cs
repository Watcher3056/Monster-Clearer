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
    public class PlayerExperienceProgressBar : MonoBehaviour
    {
        [Required]
        public Slider sliderLevelProgressBar;
        public float animSpeed = 4f;

        private void Awake()
        {
            ProcessorObserver.Default.Add(
                () => PlayerController.Current.statsPlayer.CurExperience, curExp => UpdateView(), true);
        }
        private void UpdateView()
        {
            sliderLevelProgressBar.DOValue(PlayerController.Current.statsPlayer.LevelUpProgress, 1f / animSpeed);
        }
    }
}
