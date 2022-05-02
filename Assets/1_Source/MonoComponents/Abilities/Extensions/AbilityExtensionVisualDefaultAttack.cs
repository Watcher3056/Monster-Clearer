using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TeamAlpha.Source
{
    public class AbilityExtensionVisualDefaultAttack : AbilityExtensionVisual
    {
        [Required, AssetsOnly]
        public ParticleSystem particlesHitTargetPrefab;
        [Required, AssetsOnly]
        public ParticleSystem particlesHitSourcePrefab;

        public override void Activate(Character source, Character target, bool isSecondaryTarget)
        {
            base.Activate(source, target, isSecondaryTarget);

            if (!isSecondaryTarget)
            {
                CharacterCell cellSource = PanelBattleGrid.Default.GetCellByCharacter(source);
                CharacterCell cellTarget = PanelBattleGrid.Default.GetCellByCharacter(target);

                Vector3 dirSourceTarget = cellTarget.transform.position - cellSource.transform.position;
                dirSourceTarget.Normalize();

                Vector3 startLocalPosition = cellSource.transform.localPosition;

                cellSource.transform
                    .DOPunchPosition(dirSourceTarget * 50f, 1f / DataGameMain.Default.battleAnimationSpeed, 3)
                    .OnComplete(() =>
                    {
                        cellSource.transform
                            .DOLocalMove(startLocalPosition, 0.1f)
                            .RegisterTweener();
                    })
                    .RegisterTweener();

                ParticleSystem particlesHitTarget = SpawnParticles(particlesHitTargetPrefab.gameObject);

                Vector3 positionParticlesTarget = cellTarget.transform.position;
                positionParticlesTarget.z = particlesHitTarget.transform.position.z;
                particlesHitTarget.transform.position = positionParticlesTarget;
                particlesHitTarget.Play();

                ParticleSystem particlesHitSource = SpawnParticles(particlesHitSourcePrefab.gameObject);

                Vector3 positionParticlesSource = cellSource.transform.position;
                positionParticlesSource.z = particlesHitSource.transform.position.z;
                particlesHitSource.transform.position = positionParticlesSource;

                Vector3 direction = cellTarget.transform.position - cellSource.transform.position;
                direction.Normalize();

                Quaternion lookAt = Quaternion.LookRotation(direction, Vector3.right);
                lookAt *= Quaternion.AngleAxis(90f, Vector3.down);
                particlesHitSource.transform.localRotation = lookAt;

                particlesHitSource.Play();
                //cellTarget.transform.DOPunchPosition(-dirSourceTarget * 50f, 0.25f, 3);
            }
        }
    }
}
