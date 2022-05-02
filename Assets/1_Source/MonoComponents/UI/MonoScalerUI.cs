using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeye.Actors;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class MonoScalerUI : MonoBehaviour
    {
        public enum ResizeTarget { localScale, rectSize }
        public enum ResizeTo { targetWidth, targetHeigth }
        public enum ResizeMethod { Expand, SaveInitialAspect }
        [Required]
        public RectTransform needToScale;
        [Required]
        public RectTransform scaleFrom;
        public ResizeTo resizeTo;
        public ResizeTarget resizeTarget;
        public ResizeMethod resizeMethod;

        [SerializeField, ReadOnly, ShowIf("locked")]
        private Vector2 originLocalScale;
        [SerializeField, ReadOnly, ShowIf("locked")]
        private Vector2 originRectSize;
        [SerializeField, Range(0f, 1f), ShowIf("locked"), OnValueChanged("UpdateView")]
        private float weight = 0.5f;
        [SerializeField, OnValueChanged("HandleLockChanged"), ShowIf("scaleFrom")]
        private bool locked;

        private void Awake()
        {
            UpdateView();
        }
        public void UpdateView()
        {
            if (!locked || originLocalScale == Vector2.zero || originRectSize == Vector2.zero)
                return;
            float scaleFactor = 1f;

            if (resizeMethod == ResizeMethod.Expand)
            {
                if (resizeTo == ResizeTo.targetWidth)
                    scaleFactor = ((float)scaleFrom.rect.width / needToScale.rect.width);
                else if (resizeTo == ResizeTo.targetHeigth)
                    scaleFactor = ((float)scaleFrom.rect.height / needToScale.rect.height);
            }
            else if (resizeMethod == ResizeMethod.SaveInitialAspect)
            {
                float prevAspect = originRectSize.x / originRectSize.y;
                float curAspect = needToScale.rect.width / needToScale.rect.height;
                if (resizeTo == ResizeTo.targetWidth)
                    scaleFactor = curAspect / prevAspect;
                else if (resizeTo == ResizeTo.targetHeigth)
                    scaleFactor = prevAspect / curAspect;
            }
            scaleFactor = Mathf.Lerp(1f, scaleFactor, weight);

            needToScale.localScale = Vector3.one * scaleFactor;

        }
        private void HandleLockChanged()
        {
            if (locked)
            {
                originLocalScale = needToScale.localScale;
                originRectSize = needToScale.rect.size;
                UpdateView();
            }
            else
            {
                needToScale.localScale = originLocalScale;
            }
        }
        private void OnDrawGizmos()
        {
            UpdateView();
        }
    }
}
