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
    public class PanelContextInfoWindow : MonoBehaviour
    {
        public class Info
        {
            public CharacteristicDescription characteristic;
            public TextMeshProUGUI text;

            public int CompareAndColorize(Info compareWith)
            {
                int result = characteristic.CompareTo(compareWith.characteristic);

                string textFormatted = new string(characteristic.description.ToCharArray());
                string color = "#";
                if (result == -1)
                    color += ColorUtility.ToHtmlStringRGBA(PanelContextInfo.Default.colorWorse);
                else if (result == 1)
                    color += ColorUtility.ToHtmlStringRGBA(PanelContextInfo.Default.colorBetter);
                else
                    color += ColorUtility.ToHtmlStringRGBA(PanelContextInfo.Default.colorNormal);
                color = color.ToLower();
                textFormatted = textFormatted.Replace("<color>", string.Format("<color={0}>", color));
                text.text = textFormatted;

                return result;
            }
        }

        [Required]
        public RectTransform holderContent;
        [Required]
        public TextMeshProUGUI prefabTextHeader;
        [Required]
        public TextMeshProUGUI prefabTextSeparator;
        [Required]
        public TextMeshProUGUI prefabTextElement;

        [NonSerialized]
        public List<Info> listInfo = new List<Info>();
        private RectTransform target;
        private RectTransform rectTransform;
        private void Awake()
        {
            rectTransform = transform as RectTransform;
            holderContent.DestroyAllChilds();
        }
        public TextMeshProUGUI AddTextHeader() =>
            Instantiate(prefabTextHeader, holderContent).GetComponent<TextMeshProUGUI>();
        public TextMeshProUGUI AddTextSeparator() =>
            Instantiate(prefabTextSeparator, holderContent).GetComponent<TextMeshProUGUI>();
        public TextMeshProUGUI AddTextElement() =>
            Instantiate(prefabTextElement, holderContent).GetComponent<TextMeshProUGUI>();
        public Info AddTextElement(CharacteristicDescription characteristic)
        {
            TextMeshProUGUI text = AddTextElement();
            Info info = new Info();
            info.characteristic = characteristic;
            info.text = text;

            //if (characteristic.name != null && characteristic.name != string.Empty)
            info.text.text = characteristic.DescriptionNonFormatted;
            //else
            //    info.text.text = characteristic.description;

            listInfo.Add(info);
            return info;
        }
        public void SetTarget(RectTransform rectTransform)
        {
            target = rectTransform;
            UpdatePosition();
        }
        public void UpdatePosition()
        {
            StartCoroutine(_UpdatePosition());
        }
        public IEnumerator _UpdatePosition()
        {
            //Move out of screen
            transform.position = Vector3.one * 10000f;
            yield return null;
            yield return null;

            rectTransform.pivot = new Vector2(0.5f, 0f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            Vector2 screenSize = (PanelContextInfo.Default.transform as RectTransform).rect.size;
            Vector2 offset = Vector3.up * (target.rect.size.y / 2f);
            Vector2 windowSize = rectTransform.rect.size * rectTransform.localScale;

            transform.position = target.position;
            Vector2 targetLocalPos = rectTransform.localPosition;
            Vector2 localPositionAbs = new Vector2(Mathf.Abs(targetLocalPos.x), Mathf.Abs(targetLocalPos.y));
            Vector2 diff = localPositionAbs - screenSize / 2f;
            diff += windowSize / 2f;
            diff.x = Mathf.Clamp(diff.x, 0f, diff.x);
            diff.y = Mathf.Clamp(diff.y, 0f, diff.y);

            offset.x += diff.x * (targetLocalPos.x > 0f ? -1f : 1f);
            offset.y += diff.y * (targetLocalPos.y > 0f ? -1f : 1f);

            transform.position = target.position;
            transform.localPosition += (Vector3)offset;

            yield break;
        }
    }
}
