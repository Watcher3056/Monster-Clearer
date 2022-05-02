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
        private void SetupStateMainMenu()
        {
            statesMap.AddState((int)State.MainMenu, StateMainMenuOnStart, StateMainMenuOnEnd);
        }
        private void StateMainMenuOnStart()
        {
            PanelMap.Default.panel.OpenPanel();
            PanelTop.Default.panel.OpenPanel();
            PanelBottomTabs.Default.panel.OpenPanel();
        }
        private void StateMainMenuOnEnd(int stateTo)
        {
            PanelMap.Default.panel.ClosePanel();
        }
    }
}
