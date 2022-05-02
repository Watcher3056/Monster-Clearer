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
        private void SetupStateBattle()
        {
            statesMap.AddState((int)State.Battle, StateBattleOnStart, StateBattleOnEnd);
        }
        private void StateBattleOnStart()
        {
            PanelBattleGrid.Default.panel.OpenPanel();
            PanelPlayerToolBarActions.Default.panel.OpenPanel();
            PanelTop.Default.panel.OpenPanel();
            PanelBottomTabs.Default.panel.ClosePanel();
        }
        private void StateBattleOnEnd(int stateTo)
        {
            PanelBattleGrid.Default.panel.ClosePanel();
            PanelPlayerToolBarActions.Default.panel.ClosePanel();
        }
    }
}
