using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TeamAlpha.Source
{
    public class CharacterCell : MonoBehaviour
    {
        [Required]
        public InteractableController interactable;
        public Character LinkedCharacter { get; private set; }

        public virtual void SetCharacter(Character character)
        {
            LinkedCharacter = character;
        }
        public virtual void SetActiveAbilityRangeIndicator(bool arg)
        {

        }
        public virtual void HandleDamageReceived()
        {

        }
        public virtual void UpdateView(float animSpeed)
        {

        }
        public virtual void ClearCharacter()
        {
            LinkedCharacter = null;
        }
    }
}
