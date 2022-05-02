using DG.Tweening;
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
    public class PanelMap : MonoBehaviour
    {
        public static PanelMap Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public RectTransform holder;
        [Required]
        public Button buttonPlay;
        [Required]
        public Button buttonPlayOnResting;
        [Required]
        public TextMeshProUGUI textRestTimeLeft;
        [Required]
        public CharacterCellPlayer playerCell;
        [Required]
        public ParticlesList particlesOnRest;
        [Required]
        public Transform holderPoints;
        [Required]
        public Image imageBG;
        [MinValue(0f)]
        public float maxInertiaSpeed;
        [MinValue(0f)]
        public float minInertiaSpeed;
        [Range(0f, 1f)]
        public float inertia;
        [MinValue(0f)]
        public float inertiaDrag;
        public float animSpeed = 4f;

        [SerializeField, ReadOnly, 
            InfoBox("Not all levels from DataGameMain is assigned to map", InfoMessageType.Warning
            , "@_EditorGetUnassignedLevels().Count > 0")]
        public List<PanelMapPoint> mapPoints = new List<PanelMapPoint>();
        public PanelMapPoint CurSelectedPoint { get; private set; }

        private Vector2 prevMousePos;
        private Coroutine markMoveCoroutine;
        private Vector2 CanvasCenter => new Vector2(Screen.width / 2f, Screen.height / 2f);
        private Vector2 CanvasCenterWorld => CameraManager.Default.cam.ScreenToWorldPoint(CanvasCenter);
        private float midSpeed;
        private Vector2 inertiaMove;
        private bool characterCellMoving;
        public PanelMap() => Default = this;
        private void Awake()
        {
            panel.OnPanelShow += HandlePanelShow;
            UpdateMapPointsList();
            ProcessorObserver.Default.Add(
                () => LayerDefault.Default.CurLevel, value => SelectPoint(value as LevelController), true);
            ProcessorObserver.Default.Add(
                () => PlayerController.Current.IsRestingNow, value => UpdateViewResting(), true);
            ProcessorObserver.Default.Add(
                () => PlayerController.Current.RestTimeLeft, value => UpdateViewRestingTime(), true);
            buttonPlay.onClick.AddListener(HandleButtonPlayClick);
        }
        private void Update()
        {
            ProcessScroll();
            ProcessScrollInertia();
        }
        private void ProcessScroll()
        {
            Image hit = UIManager.Default.Raycast<Image>(Input.mousePosition);
            if (hit != imageBG)
                return;
            if (Input.GetMouseButtonDown(0))
            {
                midSpeed = 0f;
                inertiaMove = Vector2.zero;
            }
            else if (Input.GetMouseButton(0) && !characterCellMoving)
            {
                Vector3 diff = (Vector2)Input.mousePosition - prevMousePos;
                diff.x = 0;
                diff /= UIManager.Default.mainCanvas.scaleFactor;
                midSpeed += diff.y * inertia;
                float curDrag = (midSpeed > 0 ? -inertiaDrag : inertiaDrag) * Time.deltaTime;
                if (Mathf.Abs(midSpeed) <= curDrag)
                    midSpeed = 0f;
                else
                    midSpeed += curDrag;
                holder.localPosition += diff;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                inertiaMove = new Vector2(0f, midSpeed * inertia);
                if (inertiaMove.magnitude > maxInertiaSpeed)
                    inertiaMove *= maxInertiaSpeed / inertiaMove.magnitude;
            }
            prevMousePos = Input.mousePosition;
        }
        private void ProcessScrollInertia()
        {
            if (characterCellMoving)
                inertiaMove = Vector2.zero;
            if (inertiaMove.magnitude > minInertiaSpeed)
            {
                if (inertiaMove.y > 0)
                    inertiaMove.y -= inertiaDrag * Time.deltaTime;
                else
                    inertiaMove.y += inertiaDrag * Time.deltaTime;
                if (inertiaMove.magnitude < 0)
                    return;
                holder.localPosition += (Vector3)inertiaMove * Time.deltaTime;
            }
        }
        private void LateUpdate()
        {
            Vector3 holderLocalPos = holder.localPosition;
            float limit = (imageBG.rectTransform.rect.height * holder.localScale.y - (transform as RectTransform).rect.height) / 2f;
            //limit *= holder.localScale.y;
            holderLocalPos.y = Mathf.Clamp(holderLocalPos.y, -limit, limit);
            holder.localPosition = holderLocalPos;
        }
#if UNITY_EDITOR
        public List<LevelController> _EditorGetUnassignedLevels()
        {
            List<LevelController> levels = new List<LevelController>(DataGameMain.Default.levels.ToArray());
            foreach (PanelMapPoint mapPoint in mapPoints)
                levels.Remove(mapPoint.LinkedLevelPrefab);
            return levels;
        }
#endif
        public void UpdateMapPointsList()
        {
            mapPoints =
                holderPoints.gameObject.GetAllComponents<PanelMapPoint>(false);
        }
        private void HandlePanelShow()
        {
            SelectPoint(LayerDefault.Default.GetLastLevelToPlayOrFinal());
        }
        private void HandleButtonPlayClick()
        {
            LayerDefault.Default.LaunchLevel(CurSelectedPoint.LinkedLevelPrefab);
        }
        private void UpdateViewResting()
        {
            if (PlayerController.Current.IsRestingNow)
            {
                buttonPlay.gameObject.SetActive(false);
                buttonPlayOnResting.gameObject.SetActive(true);
                particlesOnRest.Restart();
            }
            else
            {
                buttonPlay.gameObject.SetActive(true);
                buttonPlayOnResting.gameObject.SetActive(false);
                particlesOnRest.Stop();
            }
        }
        private void UpdateViewRestingTime()
        {
            TimeSpan timeLeft = new TimeSpan(0, 0, (int)PlayerController.Current.RestTimeLeft);
            string text = "";
            if (timeLeft.Minutes < 10)
                text += '0';
            text += timeLeft.Minutes;
            text += ':';
            if (timeLeft.Seconds < 10)
                text += '0';
            text += timeLeft.Seconds;
            textRestTimeLeft.text = text;
        }
        public void SelectPoint(LevelController levelPrefab)
        {
            SelectPoint(mapPoints.Find(p => p.LinkedLevelPrefab == levelPrefab));
        }
        public void SelectPoint(PanelMapPoint mapPoint)
        {
            characterCellMoving = true;
            CurSelectedPoint = mapPoint;
            playerCell.transform
                .DOKill();
            playerCell.transform
                .DOLocalMove(mapPoint.transform.localPosition, 1f / animSpeed)
                .OnComplete(() =>
                {
                    playerCell.transform.DOPunchPosition(Vector3.up * 30f, 3f / animSpeed, 2);
                });
            Vector2 distToPointFromCenterScreen = 
                mapPoint.transform.position - holder.transform.position;
            distToPointFromCenterScreen += ((Vector2)holder.transform.position - CanvasCenterWorld);

            holder.DOMoveY(holder.position.y - distToPointFromCenterScreen.y, 1f / animSpeed)
                .OnComplete(() =>
                {
                    characterCellMoving = false;
                });
        }
    }
}
