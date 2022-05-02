using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static TeamAlpha.Source.Character;

namespace TeamAlpha.Source
{
    public class PanelEquipmentCell : PanelPlayerToolbarActionsCell
    {
        public EquipmentSlot LinkedSlot { get; set; }
    }
}
