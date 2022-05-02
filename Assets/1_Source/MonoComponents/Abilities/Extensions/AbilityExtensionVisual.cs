using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class AbilityExtensionVisual : MonoBehaviour
    {
        public virtual void Activate(Character source, Character target, bool isSecondaryTarget)
        {
            
        }
        public ParticleSystem SpawnParticles(GameObject particlesPrefab)
        {
            ParticleSystem result = particlesPrefab.GetComponent<ParticleSystem>().InstantiateParticlesOverUI();
            result.transform.localRotation = Quaternion.identity;

            return result;
        }
    }
}
