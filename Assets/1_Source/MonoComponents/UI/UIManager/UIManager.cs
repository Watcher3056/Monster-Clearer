using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public partial class UIManager : MonoBehaviour
    {
        public static UIManager Default { get; private set; }

        [Required]
        public Canvas mainCanvas;
        [Required]
        public GraphicRaycaster graphicRaycaster;
        [Required]
        public EventSystem eventSystem;

        public enum State { None, MainMenu, Battle, Failed, Win, Inventory, Settings }

        public State CurState
        {
            get => curState;
            set
            {
                if (curState == value ||
                !statesMap.ContainsKey((int)value) ||
                !statesMap[(int)value].Condition((int)curState))
                    return;
                statesMap[(int)curState].OnEnd((int)value);
                statesMap[(int)value].OnStart();
                curState = value;

            }
        }
        private State curState;
        private Dictionary<int, StateDefault> statesMap = new Dictionary<int, StateDefault>();
        public UIManager()
        {
            Default = this;

            statesMap.AddState((int)State.None, () => { }, (a) => { });
            SetupStateMainMenu();
            SetupStateBattle();
            SetupStateFailed();
            SetupStateWin();
            SetupStateInventory();
            SetupStateSettings();
        }
        public void ExitGame()
        {
            Application.Quit();
        }
        public void SetHighlightUIElement(GameObject go, bool arg)
        {
            Canvas canvas = null;
            if (arg)
            {
                canvas = go.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 1;
            }
            else
            {
                canvas = go.GetComponent<Canvas>();
                GameObject.Destroy(canvas);
            }
        }
        public List<RaycastResult> RaycastAll(Vector2 screenPos)
        {
            PointerEventData m_PointerEventData = new PointerEventData(UIManager.Default.eventSystem);
            //Set the Pointer Event Position to that of the game object
            m_PointerEventData.position = screenPos;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            UIManager.Default.graphicRaycaster.Raycast(m_PointerEventData, results);

            return results;
        }
        public List<T> RaycastAll<T>(Vector2 screenPos) where T : UnityEngine.Component
        {
            List<RaycastResult> allRaycastResults = RaycastAll(screenPos);
            List<T> result = new List<T>();

            foreach (RaycastResult raycastResult in allRaycastResults)
            {
                T component = raycastResult.gameObject.GetComponent<T>();
                if (component != null)
                    result.Add(component);
            }

            return result;
        }
        public T Raycast<T>(Vector2 screenPos) where T: UnityEngine.Component
        {
            T result = null;
            RaycastAll(screenPos)
            .Find(r =>
            {
                result = r.gameObject.GetComponent<T>();
                return result != null;
            });
            return result;
        }
    }
}
