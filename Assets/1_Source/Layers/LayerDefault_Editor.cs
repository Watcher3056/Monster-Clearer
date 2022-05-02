using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public partial class LayerDefault
    {
#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button]
        private void CleanProgress()
        {
            ProcessorSaveLoad.CleanSaves();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                Continue();
        }
#endif
    }
}
