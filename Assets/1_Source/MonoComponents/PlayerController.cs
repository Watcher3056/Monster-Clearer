using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class PlayerController : MonoBehaviour
    {
        public static PlayerController Current { get; private set; }
        public enum PointCellsPosition { FromMouse, FromCell }

        [Required]
        public Character character;
        [Required, SerializeField]
        private Ability abilityDefaultAttack;
        public PointCellsPosition pointCellsPosition;
        public Vector2 offsetOnDrag;
        public Vector2 offsetDefaultAttack;
        [NonSerialized]
        public StatsPlayer statsPlayer = new StatsPlayer();
        public Character CurTarget { get; set; }
        public PanelPlayerToolbarActionsCell CellDraggingNow { get; set; }
        public bool AutoAttackNow => CurTarget != null && CurTarget.IsAlive && character.IsAlive;
        public int BackpackDisposeChestAmount { get; set; }
        public float RestTimeLeft { get; set; }
        public bool IsRestingNow => RestTimeLeft > 0f && IsRestEnabled;
        public bool IsRestEnabled { get; set; }

        public AbilitySimpleAttack AbilityDefaultAttack
        {
            get
            {
                return
                character.abilities
                .Find(a => abilityDefaultAttack.componentGUID.id.Equals(a.componentGUID.id)) 
                as AbilitySimpleAttack;
            }
        }

        public PlayerController() => Current = this;
        public Vector2 GetPointerScreenPosition()
        {
            if (pointCellsPosition == PointCellsPosition.FromCell)
                return CameraManager.Default.cam.WorldToScreenPoint(CellDraggingNow.interactable.transform.position);
            else
                return Input.mousePosition;
        }
        public void ResetResourceStats()
        {
            character.statsResources.healthCur = character.lvl3StatsResultSum.common.healthMax;
            character.statsResources.staminaCur = character.lvl3StatsResultSum.common.staminaMax;
        }
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;
            if (pointCellsPosition == PointCellsPosition.FromCell && CellDraggingNow == null)
                return;
            DebugExtension.DrawPoint(CameraManager.Default.cam.ScreenToWorldPoint(GetPointerScreenPosition()));
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
                if (UIManager.Default.Raycast<PanelPlayerToolbarActionsCell>(Input.mousePosition) == null)
                    PanelContextInfo.Default.HideAll();
            if (IsRestingNow)
                RestTimeLeft -= Time.deltaTime;
        }
    }
}
