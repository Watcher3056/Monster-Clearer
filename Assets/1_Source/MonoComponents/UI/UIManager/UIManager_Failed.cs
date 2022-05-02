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
        private void SetupStateFailed()
        {
            statesMap.AddState((int)State.Failed, StateFailedOnStart, StateFailedOnEnd);
        }
        private void StateFailedOnStart()
        {

        }
        private void StateFailedOnEnd(int stateTo)
        {

        }
    }
}
