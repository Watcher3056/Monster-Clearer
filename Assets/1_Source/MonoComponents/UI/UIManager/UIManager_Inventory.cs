using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public partial class UIManager
    {
        private void SetupStateInventory()
        {
            statesMap.AddState((int)State.Inventory, StateInventoryOnStart, StateInventoryOnEnd);
        }
        private void StateInventoryOnStart()
        {
            PanelEquipment.Default.panel.OpenPanel();
            PanelBackpack.Default.panel.OpenPanel();
            PanelTop.Default.panel.OpenPanel();
            PanelBottomTabs.Default.panel.OpenPanel();
        }
        private void StateInventoryOnEnd(int stateTo)
        {
            PanelEquipment.Default.panel.ClosePanel();
            PanelBackpack.Default.panel.ClosePanel();
        }
    }
}
