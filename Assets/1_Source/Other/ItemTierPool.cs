using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamAlpha.Source
{
    [Serializable, HideLabel]
    public class ItemTierPool
    {
        public List<Item> itemsPrefabs = new List<Item>();
        public int tier;
    }
}
