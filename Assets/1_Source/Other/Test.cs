using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class Test : MonoBehaviour
    {
        enum Stats
        {
            STATS_MANA,
            STATS_HEALTH,
            STATS_STAMINA,
            STATS_DEFENCE,
            STATS_ATTACK,
            STATS_DAMAGE,
            STATS_DAMAGE_REDUCTION,
            STATS_HEALTH_MAX,
            STATS_HEALTH_MIN,
            STATS_ANAL_PENETRATION_RATE,

            STATS_MAX,
        };


        private void Awake()
        {
            int[] multipliers = new int[(int)Stats.STATS_MAX];
            int[] increments = new int[(int)Stats.STATS_MAX];

            for (int i = 0; i < (int)Stats.STATS_MAX; ++i)
            {
                if (multipliers[i] > 1.0f) { /* put it whenever you want as mul*/ }
                if (increments[i] != 0.0f) { /* put it whenever you want as inc/dec*/ }
            }
        }
    }
}