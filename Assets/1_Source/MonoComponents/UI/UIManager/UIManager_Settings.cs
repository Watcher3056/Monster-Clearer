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
        private void SetupStateSettings()
        {
            statesMap.AddState((int)State.Settings, StateSettingsOnStart, StateSettingsOnEnd);
        }
        private void StateSettingsOnStart()
        {
            PanelSettings.Default.panel.OpenPanel();
            PanelTop.Default.panel.OpenPanel();
            PanelBottomTabs.Default.panel.OpenPanel();
        }
        private void StateSettingsOnEnd(int stateTo)
        {
            PanelSettings.Default.panel.ClosePanel();
        }
    }
}
