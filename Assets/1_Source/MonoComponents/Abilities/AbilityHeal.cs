using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class AbilityHeal : Ability
    {
        public StatsResources restoreAmount;

        protected override void HandleActivate(Character target, bool isSecondaryTarget)
        {
            target.ProcessHealing(CalculateHeal());
        }
        private StatsResources CalculateHeal() => restoreAmount * Power;
        protected override List<CharacteristicDescription> HandleGetDescription()
        {
            List<CharacteristicDescription> result = new List<CharacteristicDescription>();

            StatsResources _restoreAmount = CalculateHeal();
            if (_restoreAmount.healthCur != 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Health Restore";
                characteristic.valueToCompare = _restoreAmount.healthCur;
                characteristic.description =
                    string.Format("Health <color>{0}{1}</color>",
                    _restoreAmount.healthCur.GetPositiveOrNegativeSign(),
                    Mathf.Abs((int)characteristic.valueToCompare));
                characteristic.comparisonRule = CharacteristicDescription.ComparisonRule.MoreIsBetter;
                result.Add(characteristic);
            }
            if (_restoreAmount.staminaCur != 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Stamina Restore";
                characteristic.valueToCompare = _restoreAmount.staminaCur;
                characteristic.description =
                    string.Format("Stamina <color>{0}{1}</color>",
                    _restoreAmount.staminaCur.GetPositiveOrNegativeSign(),
                    Mathf.Abs((int)characteristic.valueToCompare));
                characteristic.comparisonRule = CharacteristicDescription.ComparisonRule.MoreIsBetter;
                result.Add(characteristic);
            }

            return result;
        }
    }
}
