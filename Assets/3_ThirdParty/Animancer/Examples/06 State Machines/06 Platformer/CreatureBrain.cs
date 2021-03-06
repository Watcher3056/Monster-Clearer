// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Animancer.Examples.StateMachines.Platformer
{
    /// <summary>
    /// Base class for any kind of <see cref="Platformer.Creature"/> controller - local, network, AI, replay, etc.
    /// </summary>
    [HelpURL(Strings.APIDocumentationURL + ".Examples.StateMachines.Platformer/CreatureBrain")]
    public abstract class CreatureBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Creature _Creature;
        public Creature Creature { get { return _Creature; } }

        /************************************************************************************************************************/

        public float MovementDirection { get; protected set; }

        public bool IsRunning { get; protected set; }

        /************************************************************************************************************************/
    }
}
