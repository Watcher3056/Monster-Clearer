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
        private void SetupStateWin()
        {
            statesMap.AddState((int)State.Win, StateWinOnStart, StateWinOnEnd);
        }
        private void StateWinOnStart()
        {

        }
        private void StateWinOnEnd(int stateTo)
        {

        }
    }
}
