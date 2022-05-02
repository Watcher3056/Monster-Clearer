using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    public class PanelLevelCompleteFailed : PanelLevelComplete
    {
        public static PanelLevelCompleteFailed Default { get; private set; }

        public PanelLevelCompleteFailed() => Default = this;
    }
}
