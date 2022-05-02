using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    [InlineEditor, GUIColor("@$value.colorBG")]
    public class ItemRarityType : ScriptableObject
    {
        [HideLabel, HideInInlineEditors]
        public ComponentGUID guid;

        public string Name => _name;
        [Required, OnValueChanged("_EditorOnValueNameChanged"), SerializeField]
        private string _name;
        [HideIf("IsDefaultRarityType")]
        public Color colorBG = Color.white;
        [MinMaxSlider(0f, 2f, true)]
        public Vector2 statsMultiplier;
        [Range(0f, 1f), HideIf("IsDefaultRarityType")]
        public float probability;

        public bool IsDefaultRarityType => DataGameMain.Default.itemRarityTypeDefault == this;

#if UNITY_EDITOR
        private void _EditorOnValueNameChanged()
        {
            name = "Item Rarity Type " + _name;
            UnityEditor.EditorUtility.SetDirty(this);

            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            Debug.Log(UnityEditor.AssetDatabase.RenameAsset(assetPath, name));
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}
