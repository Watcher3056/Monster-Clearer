using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class AbilitySimpleAttack : Ability
    {
        public enum DamageSourceStat { damagePhysical, damageMagic }

        public DamageSourceStat damageSourceStat;
        public StatsResourcesModifier damage;
        public int stunAmount;
        public float mainTargetDamageMod = 1f;
        public float secondTargetsDamageMod = 1f;
        protected override void HandleActivate(Character target, bool isSecondaryTarget)
        {
            target.ProcessDamage(Owner, GetAllDamage(isSecondaryTarget));
            NPC npc = target.GetComponent<NPC>();
            if (npc != null)
                npc.CooldownLeft = Mathf.Clamp(npc.CooldownLeft + stunAmount, 0, 2);

        }
        public StatsResources GetAllDamage(bool isSecondaryTarget)
        {
            StatsResourcesModifier modifier = GetDamageBaseModifier(isSecondaryTarget);
            StatsResources result = new StatsResources();

            if (damageSourceStat == DamageSourceStat.damagePhysical)
            {
                result.healthCur = Owner.lvl3StatsResultSum.common.damage;
                result.staminaCur = Owner.lvl3StatsResultSum.common.damage;
            }
            else if (damageSourceStat == DamageSourceStat.damageMagic)
            {
                result.healthCur = Owner.lvl3StatsResultSum.main.magic;
                result.staminaCur = Owner.lvl3StatsResultSum.main.magic;
            }
            return result.ModifyIncludeOrigin(modifier);
        }
        private StatsResourcesModifier GetDamageBaseModifier(bool isSecondaryTarget)
        {
            return damage * Power * (isSecondaryTarget ? secondTargetsDamageMod : mainTargetDamageMod);
        }
        protected override List<CharacteristicDescription> HandleGetDescription()
        {
            List<CharacteristicDescription> result = new List<CharacteristicDescription>();

            StatsResourcesModifier damageMainTarget = GetDamageBaseModifier(false);
            StatsResourcesModifier damageSecondTarget = GetDamageBaseModifier(true);
            if (damage.increment.healthCur != 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Damage Constant";
                if (multiTarget)
                {
                    characteristic.valueToCompare =
                        (int)((damageMainTarget.increment.healthCur + damageSecondTarget.increment.healthCur) / 2f);

                    if (damageMainTarget.increment.healthCur != damageSecondTarget.increment.healthCur)
                        characteristic.description =
                            string.Format("Deals <color>{0}</color> damage to main target and <color>{1}</color> to others",
                            (int)damageMainTarget.increment.healthCur, (int)damageSecondTarget.increment.healthCur);
                    else
                        characteristic.description =
                            string.Format("Deals <color>{0}</color> damage to all targets",
                            (int)damageMainTarget.increment.healthCur);
                }
                else
                {
                    characteristic.valueToCompare = (int)damageMainTarget.increment.healthCur;
                    characteristic.description =
                        string.Format("Deals <color>{0}</color> damage",
                        (int)damageMainTarget.increment.healthCur);
                }
                result.Add(characteristic);
            }
            if (damageMainTarget.multiply.healthCur != 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Damage";
                string textDamageSource = "";
                if (damageSourceStat == DamageSourceStat.damagePhysical)
                    textDamageSource = "Attack";
                else
                    textDamageSource = "Magic";
                if (multiTarget)
                {
                    characteristic.valueToCompare =
                        (int)((damageMainTarget.multiply.healthCur + damageSecondTarget.multiply.healthCur) / 2f * 100f);
                    if (damageMainTarget.multiply.healthCur != damageSecondTarget.multiply.healthCur)
                        characteristic.description =
                            string.Format("Deals <color>{0:P0}</color> to main target\n" +
                                          "of {1} and <color>{2:P0}</color> to others",
                            damageMainTarget.multiply.healthCur, textDamageSource, damageSecondTarget.multiply.healthCur);
                    else
                        characteristic.description =
                            string.Format("Deals <color>{0:P0}</color> of {1} to all targets",
                            damageMainTarget.multiply.healthCur, textDamageSource);
                }
                else
                {
                    characteristic.valueToCompare = (int)(damageMainTarget.multiply.healthCur * 100f);
                    characteristic.description =
                        string.Format("Deals <color>{0:P0}</color> of {1}",
                        damageMainTarget.multiply.healthCur, textDamageSource);
                }
                result.Add(characteristic);
            }
            if (stunAmount != 0)
            {
                CharacteristicDescription characteristic = new CharacteristicDescription();
                characteristic.nameToCompare = "Stun";
                characteristic.valueToCompare = stunAmount;
                characteristic.description = 
                    string.Format("Stun for <color>{0}</color> rounds", 
                    characteristic.valueToCompare);
                result.Add(characteristic);
            }

            return result;
        }
    }
}