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
    public class PlayerCurrentLevelIndicator : MonoBehaviour
    {
        [Required]
        public TextMeshProUGUI textPlayerLevel;

        private void Awake()
        {
            ProcessorObserver.Default.Add(
                () => PlayerController.Current.statsPlayer.curLevel, curLevel => UpdateView(), true);
        }
        private void UpdateView()
        {
            textPlayerLevel.text = PlayerController.Current.statsPlayer.curLevel.ToString();
        }
    }
}
