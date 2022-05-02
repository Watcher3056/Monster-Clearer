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
    public class PanelTopStatIndicator : MonoBehaviour
    {
        [Required]
        public Button buttonIncrease;
        [Required]
        public TextMeshProUGUI textCounter;

        public event Action OnIncrease = () => { };
        private void Awake()
        {
            buttonIncrease.onClick.AddListener(HandleButtonIncreaseClick);
            ProcessorObserver.Default.Add(() => PlayerController.Current.statsPlayer.skillPointsLeft, value => UpdateView(), true);
        }
        private void HandleButtonIncreaseClick()
        {
            OnIncrease.Invoke();
            PlayerController.Current.statsPlayer.skillPointsLeft--;
            PlayerController.Current.character.RecountStatsAll();
            UpdateView();
        }
        public void UpdateView(int curValue)
        {
            textCounter.text = curValue.ToString();

            UpdateView();
        }
        public void UpdateView()
        {
            if (PlayerController.Current.statsPlayer.skillPointsLeft > 0)
                buttonIncrease.interactable = true;
            else
                buttonIncrease.interactable = false;
        }
    }
}
