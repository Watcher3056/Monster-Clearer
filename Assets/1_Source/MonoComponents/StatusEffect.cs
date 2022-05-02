using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixeye.Actors;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class StatusEffect : MonoBehaviour
    {
        [HideLabel, Title("Component GUID")]
        public ComponentGUID componentGUID;
        [Title("Settings")]
        public int priority;
        public int lifeTime;

        [ReadOnly]
        public Character source;
        [ReadOnly]
        private Character target;

        public Character Target
        {
            get => target;
            set
            {
                target = value;
                target.statusEffects.Add(this);

                HandleTargetChanged();
            }
        }
        private void Start()
        {
            HandleStart();
        }
        protected virtual void HandleStart()
        {

        }
        protected virtual void HandleTargetChanged()
        {

        }
        public void OnRecountTargetStats()
        {

            HandleRecountTargetStats();
        }
        protected virtual void HandleRecountTargetStats()
        {

        }
        public void OnTurn()
        {
            HandleTurn();

            lifeTime--;
            if (lifeTime <= 0)
                HandleLifeTimeEnd();
        }
        protected virtual void HandleTurn()
        {

        }
        protected virtual void HandleLifeTimeEnd()
        {

        }
    }
}
