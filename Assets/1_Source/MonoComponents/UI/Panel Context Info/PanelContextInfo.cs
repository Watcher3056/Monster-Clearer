using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static TeamAlpha.Source.PanelContextInfoWindow;

namespace TeamAlpha.Source
{
    public class PanelContextInfo : MonoBehaviour
    {
        public static PanelContextInfo Default { get; private set; }

        [Required]
        public Panel panel;
        [Required]
        public PanelContextInfoWindow prefabWindow;
        [Required]
        public RectTransform holderContent;
        public Color colorNormal = Color.white;
        public Color colorBetter = Color.green;
        public Color colorWorse = Color.red;
        public float animSpeed = 4f;

        public PanelContextInfo() => Default = this;
        private List<PanelContextInfoWindow> windows = new List<PanelContextInfoWindow>();
        private void Awake()
        {
            holderContent.DestroyAllChilds();
        }
        private PanelContextInfoWindow InstantiateWindow(RectTransform target)
        {
            PanelContextInfoWindow window =
                Instantiate(prefabWindow.gameObject, holderContent).GetComponent<PanelContextInfoWindow>();
            window.SetTarget(target);
            windows.Add(window);
            return window;
        }
        public void ShowComparison(PanelPlayerToolbarActionsCell cellSelected,
            List<PanelPlayerToolbarActionsCell> cellsToCompare)
        {
            HideAll();
            PanelContextInfoWindow windowSelected = ShowInfo(cellSelected);
            List<PanelContextInfoWindow> windowsOther = new List<PanelContextInfoWindow>();
            foreach (PanelPlayerToolbarActionsCell cell in cellsToCompare)
            {
                windowsOther.Add(ShowInfo(cell));
            }

            List<Info> allInfo = new List<Info>();
            foreach (PanelContextInfoWindow window in windowsOther)
                allInfo.AddRange(window.listInfo.ToArray());
            allInfo.AddRange(windowSelected.listInfo.ToArray());

            while (allInfo.Count > 0)
            {
                List<Info> infoSameName = new List<Info>();
                infoSameName.AddRange(allInfo.FindAll(i => i.characteristic.nameToCompare == allInfo[0].characteristic.nameToCompare));


                infoSameName.Sort((info1, info2) => info1.characteristic.CompareTo(info2.characteristic));
                infoSameName.Reverse();
                Info infoSelectedItem = 
                    windowSelected.listInfo.Find(i => i.characteristic.nameToCompare == allInfo[0].characteristic.nameToCompare);

                allInfo.RemoveAll(i => infoSameName.Contains(i));
                if (infoSameName.Count <= 1)
                    continue;
                if (infoSelectedItem == null)
                    continue;

                for (int i = 0; i < infoSameName.Count; i++)
                {
                    Info info = infoSameName[i];
                    if (info == infoSelectedItem)
                        continue;

                    int result = info.CompareAndColorize(infoSelectedItem);
                }

                //Compare with most poor item non including selected
                infoSelectedItem.CompareAndColorize(infoSameName.FindLast(i => i != infoSelectedItem));
            }
        }
        public void HideAll()
        {
            foreach (PanelContextInfoWindow window in windows)
            {
                Destroy(window.gameObject);
            }
            windows.Clear();
        }
        public PanelContextInfoWindow ShowInfo(PanelPlayerToolbarActionsCell cell)
        {
            transform.SetAsLastSibling();
            PanelContextInfoWindow window = InstantiateWindow(cell.transform as RectTransform);
            if (cell.linkedItem != null)
            {
                TextMeshProUGUI textHeader = window.AddTextHeader();
                textHeader.text = cell.linkedItem.itemName;


                float H, S, V;

                Color.RGBToHSV(cell.linkedItem.RarityType.colorBG, out H, out S, out V);
                S *= 1.2f;

                VertexGradient gradient = new VertexGradient();
                Color colorTop = Color.HSVToRGB(H, S, V, true);
                Color colorBottom = cell.linkedItem.RarityType.colorBG;

                gradient.topLeft = colorTop;
                gradient.topRight = colorTop;
                gradient.bottomLeft = Color.white;
                gradient.bottomRight = Color.white;

                textHeader.color = colorBottom;
                textHeader.colorGradient = gradient;
                AddItemInfo(window, cell.linkedItem);
            }
            else if (cell.linkedAbility != null)
            {
                window.AddTextHeader().text = cell.linkedAbility.abilityName;
                AddAbilityInfo(window, cell.linkedAbility);
            }

            return window;
        }
        public void ShowInfo(CharacterCellNPC cell)
        {
            transform.SetAsLastSibling();
            PanelContextInfoWindow window = InstantiateWindow(cell.transform as RectTransform);
            window.AddTextHeader().text = cell.LinkedNPC.character.characterName;

            List<CharacteristicDescription> characteristics = cell.LinkedNPC.GetDescription();

            foreach (CharacteristicDescription characteristic in characteristics)
            {
                window.AddTextElement(characteristic);
            }
        }
        private void AddAbilityInfo(PanelContextInfoWindow window, Ability ability)
        {
            List<CharacteristicDescription> characteristics = ability.GetDescription();

            foreach (CharacteristicDescription characteristic in characteristics)
                window.AddTextElement(characteristic);
        }
        private void AddItemInfo(PanelContextInfoWindow window, Item item)
        {

            if (item.ability != null)
            {
                window.AddTextSeparator().text = "Active";
                AddAbilityInfo(window, item.ability);
            }

            List<CharacteristicDescription> characteristics = item.GetDescription();

            if (characteristics.Count > 0)
            {
                window.AddTextSeparator().text = "Passive";
                foreach (CharacteristicDescription characteristic in characteristics)
                {
                    window.AddTextElement(characteristic);
                }
            }
        }
    }
}
