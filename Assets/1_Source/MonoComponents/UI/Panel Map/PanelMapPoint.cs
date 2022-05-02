using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class PanelMapPoint : MonoBehaviour
    {
        [Required]
        public Button buttonCompleted;
        [Required]
        public Button buttonNotCompleted;
        [Required]
        public GameObject viewCompleted;
        [Required]
        public GameObject viewNotCompleted;
        [Required]
        public TextMeshProUGUI textEnemiesAmount;
        [Required, ValueDropdown("_EditorLevelsDropDown"), OnValueChanged("UpdateViewEnemiesAmount"),
            ValidateInput("_EditorValidateInputLinkedLevelGUID", "@_EditorGUIDErrorMessage"),
            ShowIf("panelMap"), ShowInInspector]
        private string LinkedLevelGUID
        {
            get => DataGameMain.Default.levels.Find(l => l.guid.id.Equals(linkedLevelGUID))?.name;
            set => linkedLevelGUID = value;
        }
        [HideInInspector]
        public string linkedLevelGUID;
        [ReadOnly]
        public PanelMap panelMap;

        public LevelController LinkedLevelPrefab =>
            DataGameMain.Default.levels.Find(l => l.guid.id.Equals(linkedLevelGUID));
#if UNITY_EDITOR
        private string _EditorGUIDErrorMessage => "Level with guid [" + linkedLevelGUID + "] does not exist";
        private IEnumerable _EditorLevelsDropDown
        {
            get
            {
                ValueDropdownList<string> guids = new ValueDropdownList<string>();
                if (panelMap == null)
                    return guids;

                List<LevelController> levels = panelMap._EditorGetUnassignedLevels();
                while (levels.Count > 0)
                {
                    LevelController level = levels[0];
                    levels.RemoveAt(0);

                    if (panelMap)
                        guids.Add(new ValueDropdownItem<string>
                        {
                            Text = level.name,
                            Value = level.guid.id
                        });
                }
                return guids;
            }
        }
        private bool _EditorValidateInputLinkedLevelGUID(string guid)
        {
            bool result = false;
            if (panelMap != null)
                result = DataGameMain.Default.levels.Exists(l => l.guid.id == linkedLevelGUID);

            return result;
        }
        [OnInspectorInit]
        private void OnInspectorInit()
        {
            panelMap = transform.GetComponentInParent<PanelMap>();
            if (panelMap != null && !panelMap.mapPoints.Contains(this))
                panelMap.UpdateMapPointsList();
        }
#endif

        private void Awake()
        {
            ProcessorObserver.Default.Add(() => LinkedLevelPrefab.Unlocked, unlocked => UpdateView(), true);
            ProcessorObserver.Default.Add(() => LinkedLevelPrefab.LevelCompleted, completed => UpdateView(), true);

            //buttonCompleted.onClick.AddListener(HandleButtonClick);
            buttonNotCompleted.onClick.AddListener(HandleButtonClick);
        }
        public void UpdateView()
        {
            if (LinkedLevelPrefab.Unlocked)
            {
                buttonCompleted.interactable = true;
                buttonNotCompleted.interactable = true;
            }
            else
            {
                buttonCompleted.interactable = false;
                buttonNotCompleted.interactable = false;
            }

            if (LinkedLevelPrefab.LevelCompleted)
            {
                viewNotCompleted.SetActive(false);
                viewCompleted.SetActive(true);
            }
            else
            {
                viewNotCompleted.SetActive(true);
                viewCompleted.SetActive(false);
            }
            UpdateViewEnemiesAmount();
        }
        public void UpdateViewEnemiesAmount()
        {
            textEnemiesAmount.text = LinkedLevelPrefab?.enemies.Count.ToString();
        }
        private void HandleButtonClick()
        {
            PanelMap.Default.SelectPoint(this);
        }
    }
}
