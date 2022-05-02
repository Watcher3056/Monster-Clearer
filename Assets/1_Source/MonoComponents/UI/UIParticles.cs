using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class UIParticles : MonoBehaviour
    {
        [Required]
        public ParticleSystem particles;
        [Required]
        public List<GameObject> objectsOver;

        private void Awake()
        {
            particles.SetParticlesOverUI(5);

            foreach (GameObject go in objectsOver)
            {
                go.SetUILevelZ(6);
            }
        }
    }
}
