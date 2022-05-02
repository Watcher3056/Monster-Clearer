using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TeamAlpha.Source
{
    [Serializable]
    public class Stats
    {
        [Serializable]
        public class StatsMain
        {
            public float attack;
            public float stamina;
            public float magic;
            public float defence;
            public float health;

            public int StatsSum => (int)(attack + stamina + magic + defence + health);
            public StatsCommon GetCommonStatsByConversion()
            {
                StatsCommon result = new StatsCommon();
                result.damage = DataGameMain.Default.attackDamagePerPoint * attack;
                result.damageReduction = DataGameMain.Default.defenceDamageReductionPerPoint * defence;
                result.healthMax = health;
                result.staminaMax = DataGameMain.Default.staminaPerPoint * stamina;

                return result;
            }
            public static StatsMain operator +(StatsMain arg1, StatsMain arg2)
            {
                StatsMain result = new StatsMain();

                result.attack = arg1.attack + arg2.attack;
                result.stamina = arg1.stamina + arg2.stamina;
                result.magic = arg1.magic + arg2.magic;
                result.defence = arg1.defence + arg2.defence;
                result.health = arg1.health + arg2.health;

                return result;
            }
            public static StatsMain operator +(StatsMain arg1, float arg2)
            {
                StatsMain result = new StatsMain();

                result.attack = arg1.attack + arg2;
                result.stamina = arg1.stamina + arg2;
                result.magic = arg1.magic + arg2;
                result.defence = arg1.defence + arg2;
                result.health = arg1.health + arg2;

                return result;
            }
            public static StatsMain operator -(StatsMain arg1, StatsMain arg2)
            {
                StatsMain result = new StatsMain();

                result.attack = arg1.attack - arg2.attack;
                result.stamina = arg1.stamina - arg2.stamina;
                result.magic = arg1.magic - arg2.magic;
                result.defence = arg1.defence - arg2.defence;
                result.health = arg1.health - arg2.health;

                return result;
            }
            public static StatsMain operator -(StatsMain arg1, float arg2)
            {
                StatsMain result = new StatsMain();

                result.attack = arg1.attack - arg2;
                result.stamina = arg1.stamina - arg2;
                result.magic = arg1.magic - arg2;
                result.defence = arg1.defence - arg2;
                result.health = arg1.health - arg2;

                return result;
            }
            public static StatsMain operator *(StatsMain arg1, StatsMain arg2)
            {
                StatsMain result = new StatsMain();

                result.attack = arg1.attack * arg2.attack;
                result.stamina = arg1.stamina * arg2.stamina;
                result.magic = arg1.magic * arg2.magic;
                result.defence = arg1.defence * arg2.defence;
                result.health = arg1.health * arg2.health;

                return result;
            }
            public static StatsMain operator *(StatsMain arg1, float arg2)
            {
                StatsMain result = new StatsMain();

                result.attack = (int)(arg1.attack * arg2);
                result.stamina = (int)(arg1.stamina * arg2);
                result.magic = (int)(arg1.magic * arg2);
                result.defence = (int)(arg1.defence * arg2);
                result.health = (int)(arg1.health * arg2);

                return result;
            }
        }
        [Serializable]
        public class StatsCommon
        {
            public float damage;
            public float damageReduction;
            public float healthMax;
            public float staminaMax;


            public static StatsCommon operator +(StatsCommon arg1, StatsCommon arg2)
            {
                StatsCommon result = new StatsCommon();

                result.damage = arg1.damage + arg2.damage;
                result.damageReduction = arg1.damageReduction + arg2.damageReduction;
                result.healthMax = arg1.healthMax + arg2.healthMax;
                result.staminaMax = arg1.staminaMax + arg2.staminaMax;

                return result;
            }
            public static StatsCommon operator +(StatsCommon arg1, float arg2)
            {
                StatsCommon result = new StatsCommon();

                result.damage = arg1.damage + arg2;
                result.damageReduction = arg1.damageReduction + arg2;
                result.healthMax = arg1.healthMax + arg2;
                result.staminaMax = arg1.staminaMax + arg2;

                return result;
            }
            public static StatsCommon operator -(StatsCommon arg1, StatsCommon arg2)
            {
                StatsCommon result = new StatsCommon();

                result.damage = arg1.damage - arg2.damage;
                result.damageReduction = arg1.damageReduction - arg2.damageReduction;
                result.healthMax = arg1.healthMax - arg2.healthMax;
                result.staminaMax = arg1.staminaMax - arg2.staminaMax;

                return result;
            }
            public static StatsCommon operator -(StatsCommon arg1, float arg2)
            {
                StatsCommon result = new StatsCommon();

                result.damage = arg1.damage - arg2;
                result.damageReduction = arg1.damageReduction - arg2;
                result.healthMax = arg1.healthMax - arg2;
                result.staminaMax = arg1.staminaMax - arg2;

                return result;
            }
            public static StatsCommon operator *(StatsCommon arg1, StatsCommon arg2)
            {
                StatsCommon result = new StatsCommon();

                result.damage = arg1.damage * arg2.damage;
                result.damageReduction = arg1.damageReduction * arg2.damageReduction;
                result.healthMax = arg1.healthMax * arg2.healthMax;
                result.staminaMax = arg1.staminaMax * arg2.staminaMax;

                return result;
            }
            public static StatsCommon operator *(StatsCommon arg1, float arg2)
            {
                StatsCommon result = new StatsCommon();

                result.damage = arg1.damage * arg2;
                result.damageReduction = arg1.damageReduction * arg2;
                result.healthMax = arg1.healthMax * arg2;
                result.staminaMax = arg1.staminaMax * arg2;

                return result;
            }
        }


        public StatsMain main = new StatsMain();
        public StatsCommon common = new StatsCommon();

        public static Stats operator +(Stats arg1, Stats arg2)
        {
            Stats result = new Stats();
            result.main = arg1.main + arg2.main;
            result.common = arg1.common + arg2.common;

            return result;
        }
        public static Stats operator +(Stats arg1, float arg2)
        {
            Stats result = new Stats();
            result.main = arg1.main + arg2;
            result.common = arg1.common + arg2;

            return result;
        }
        public static Stats operator -(Stats arg1, Stats arg2)
        {
            Stats result = new Stats();
            result.main = arg1.main - arg2.main;
            result.common = arg1.common - arg2.common;

            return result;
        }
        public static Stats operator -(Stats arg1, float arg2)
        {
            Stats result = new Stats();
            result.main = arg1.main - arg2;
            result.common = arg1.common - arg2;

            return result;
        }
        public static Stats operator *(Stats arg1, Stats arg2)
        {
            Stats result = new Stats();
            result.main = arg1.main * arg2.main;
            result.common = arg1.common * arg2.common;

            return result;
        }
        public static Stats operator *(Stats arg1, float arg2)
        {
            Stats result = new Stats();
            result.main = arg1.main * arg2;
            result.common = arg1.common * arg2;

            return result;
        }
    }
    [Serializable]
    public class StatsResources
    {
        public float healthCur;
        public float staminaCur;

        public static StatsResources operator +(StatsResources arg1, StatsResources arg2)
        {
            StatsResources result = new StatsResources();
            result.healthCur = arg1.healthCur + arg2.healthCur;
            result.staminaCur = arg1.staminaCur + arg2.staminaCur;

            return result;
        }
        public static StatsResources operator -(StatsResources arg1, StatsResources arg2)
        {
            StatsResources result = new StatsResources();
            result.healthCur = arg1.healthCur - arg2.healthCur;
            result.staminaCur = arg1.staminaCur - arg2.staminaCur;

            return result;
        }
        public static StatsResources operator *(StatsResources arg1, StatsResources arg2)
        {
            StatsResources result = new StatsResources();
            result.healthCur = arg1.healthCur * arg2.healthCur;
            result.staminaCur = arg1.staminaCur * arg2.staminaCur;

            return result;
        }
        public static StatsResources operator *(StatsResources arg1, float arg2)
        {
            StatsResources result = new StatsResources();
            result.healthCur = arg1.healthCur * arg2;
            result.staminaCur = arg1.staminaCur * arg2;

            return result;
        }
    }
    [Serializable]
    public class StatsPlayer
    {
        public int curLevel = 1;
        public int skillPointsLeft;

        public int CurExperience
        {
            get => curExperience;
            set
            {
                curExperience = value;
                while (curExperience >= ExpRequiredToNextLevel)
                {
                    curExperience -= ExpRequiredToNextLevel;
                    curLevel++;
                    skillPointsLeft += DataGameMain.Default.skillPointsPerPlayerLevel;
                }
            }
        }
        [SerializeField]
        private int curExperience;
        public int ExpRequiredToNextLevel
        {
            get
            {
                float multiplier = Mathf.Pow(DataGameMain.Default.levelCostIncrease, curLevel);
                int result = (int)(DataGameMain.Default.firstCharacterLevelExpCost * multiplier);

                return result;
            }
        }
        public float LevelUpProgress => (float)curExperience / ExpRequiredToNextLevel;
    }

    [Serializable]
    public class StatsModifier
    {
        public Stats increment;
        public Stats multiply = new Stats
        {
            main = new Stats.StatsMain
            {
                magic = 1,
                health = 1,
                stamina = 1,
                defence = 1,
                attack = 1
            },
            common = new Stats.StatsCommon
            {
                damage = 1,
                damageReduction = 1,
                healthMax = 1,
                staminaMax = 1
            }
        };
        public static StatsModifier operator *(StatsModifier arg1, float arg2)
        {
            StatsModifier result = new StatsModifier();
            result.increment = arg1.increment * arg2;
            result.multiply = (arg1.multiply - 1f) * arg2 + 1f;

            return result;
        }
        //public List<CharacteristicDescription> GetContextInfo()
        //{
        //    List<CharacteristicDescription> result = new List<CharacteristicDescription>();
        //    List<FieldInfo> fieldsInfo = new List<FieldInfo>();
        //    fieldsInfo.AddRange(increment.main.GetType().GetFields());
        //    fieldsInfo.AddRange(increment.common.GetType().GetFields());
        //    foreach (FieldInfo fieldInfo in fieldsInfo)
        //    {
        //        CharacteristicDescription description = new CharacteristicDescription();
        //        description.value = fieldInfo.GetValue(fieldInfo.;
        //    }
        //    fieldsInfo.AddRange(multiply.main.GetType().GetFields());
        //    fieldsInfo.AddRange(multiply.common.GetType().GetFields());
        //}
    }
    public struct CharacteristicDescription : IComparable<CharacteristicDescription>
    {
        public enum ComparisonRule { MoreIsBetter, LessIsBetter }
        public string nameToCompare;
        public string description;
        public string DescriptionNonFormatted
        {
            get
            {
                string result = new string(description.ToCharArray());

                result = result
                    .Replace("<color>", "")
                    .Replace("</color>", "");

                return result;
            }
        }

        public float valueToCompare;
        public ComparisonRule comparisonRule;

        public int CompareTo(CharacteristicDescription other)
        {
            if (other.nameToCompare != this.nameToCompare)
                return 0;
            if (comparisonRule == ComparisonRule.MoreIsBetter)
                return valueToCompare.CompareTo(other.valueToCompare);
            else
                return valueToCompare.CompareTo(other.valueToCompare) * -1;
        }
    }
    [Serializable]
    public class StatsResourcesModifier
    {
        public StatsResources increment;
        public StatsResources multiply = new StatsResources
        {
            healthCur = 1f,
            staminaCur = 0f,
        };

        public static StatsResourcesModifier operator *(StatsResourcesModifier arg1, float arg2)
        {
            StatsResourcesModifier result = new StatsResourcesModifier();
            result.increment = new StatsResources();
            result.increment = arg1.increment * arg2;
            result.multiply = arg1.multiply * arg2;

            return result;
        }
    }


    public static class StatsExtensions
    {
        public static Stats ModifyIncludeOrigin(this Stats origin, StatsModifier modifier)
        {
            return origin * modifier.multiply + modifier.increment;
        }
        public static Stats ModifyExcludeOrigin(this Stats origin, StatsModifier modifier)
        {
            return (origin * modifier.multiply - origin) + modifier.increment;
        }
        public static StatsResources ModifyIncludeOrigin(this StatsResources origin, StatsResourcesModifier modifier)
        {
            return origin * modifier.multiply + modifier.increment;
        }
        public static StatsResources ModifyExcludeOrigin(this StatsResources origin, StatsResourcesModifier modifier)
        {
            return (origin * modifier.multiply - origin) + modifier.increment;
        }
    }
}