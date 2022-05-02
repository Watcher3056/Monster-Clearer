using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Pixeye.Actors;
using UnityEngine.EventSystems;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class InteractableController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler,
        IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {

        #region SETTINGS
        public bool rotating;
        public bool physicsMove;
        #endregion

        #region RUNTIME

        public bool Interactable
        {
            set
            {
                interactable = value;
                //if (collider)
                //    collider.enabled = value;
                isDrag = false;
            }
            get => interactable;
        }
        public bool Draggable
        {
            set
            {
                draggable = value;
                isDrag = false;
            }
            get => Interactable && draggable;
        }
        public bool IsDrag => isDrag;
        public event Action OnDragStart = () => { };
        public event Action OnDragEnd = () => { };
        public event Action OnClick = () => { };
        public Vector3 Offset { get; set; }

        //[HideInInspector]
        //public new Collider2D collider;
        //[HideInInspector]
        //public new Rigidbody2D rigidbody2D;
        [ShowInInspector, ReadOnly, DetailedInfoBox("Troubleshoot...", "Require collider2D(trigger) + SpriteRenderer or Image")]
        private bool isDrag;
        [ShowInInspector, ReadOnly]
        private bool interactable = true;
        private bool draggable = false;

        public bool PositionChanged { get; private set; }
        public bool RotationChanged { get; private set; }
        private Vector2 startDragMousePos;
        private Vector2 startBodyPos;
        private Vector2 prevBodyPos;
        private Vector2 cameraPosOnPointerDown;
        private RectTransform rect;
        private Canvas canvas;
        private float startAngleDiff;
        private float prevAngle;
        #endregion
        private void OnEnable()
        {
            prevBodyPos = transform.localPosition;
            prevAngle = transform.rotation.z;
            canvas = GetComponentInParent<Canvas>();
            //collider = GetComponent<Collider2D>();
            //rigidbody2D = GetComponent<Rigidbody2D>();
            rect = GetComponent<RectTransform>();
        }
        private void Update()
        {
            try
            {

                Vector2 curPos = Vector2.zero;
                curPos = rect.transform.localPosition;
                RotationChanged = prevAngle != rect.transform.rotation.z;
                prevAngle = rect.transform.rotation.z;
                PositionChanged = prevBodyPos != curPos;
                prevBodyPos = curPos;

                if (!isDrag || !Interactable || !Draggable)
                {
                    return;
                }
                Vector3 mouseWorldPosition = CameraManager.Default.cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 position = mouseWorldPosition + Offset;
                position.z = rect.transform.position.z;

                rect.transform.position = position;
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Draggable)
                return;
            if (canvas != null)
            {
                startBodyPos = rect.transform.position;
                startDragMousePos = UIHelper.GetScreenSpaceOverlayMousePosition(canvas);
            }
            else
            {
                startBodyPos = transform.position;
                startDragMousePos = CameraManager.Default.cam.ScreenToWorldPoint(Input.mousePosition);
                startAngleDiff = MathHelper.SignedAngle2D(startBodyPos, startDragMousePos, startBodyPos + (Vector2)transform.up);
            }
            isDrag = true;
            OnDragStart();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Draggable)
                return;
            isDrag = false;
            OnDragEnd();
        }

        public void OnDrag(PointerEventData eventData)
        {

        }

        public void OnPointerClick(PointerEventData eventData)
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            cameraPosOnPointerDown = CameraManager.Default.cam.transform.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Interactable &&
                Mathf.Abs(cameraPosOnPointerDown.x -
                CameraManager.Default.cam.transform.position.x) < 0.1f)
            {
                if ((canvas != null && !isDrag) || canvas == null)
                    OnClick.Invoke();
            }
            else
                this.Log("Is not interactable!");
        }
    }
}
