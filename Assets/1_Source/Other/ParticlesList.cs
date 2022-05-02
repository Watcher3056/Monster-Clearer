using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    [Serializable, HideLabel]
    public class ParticlesList
    {
        [Required]
        public List<ParticleSystem> particles;

        public void Restart()
        {
            Stop();
            Play();
        }
        public void Play()
        {
            foreach (ParticleSystem p in particles)
                p.Play();
        }
        public void Stop()
        {
            foreach (ParticleSystem p in particles)
                p.Stop();
        }
    }
}
