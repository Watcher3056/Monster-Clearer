using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public class PanelLevelCompleteWin : PanelLevelComplete
    {
        public static PanelLevelCompleteWin Default { get; private set; }

        public PanelLevelCompleteWin() => Default = this;
    }
}
