using System;
using UnityEngine;
using ZooWorld.Animals.Movement;
using ZooWorld.World;

namespace ZooWorld.Animals.Definitions
{
    [CreateAssetMenu(fileName = "JumpMovement", menuName = "Zoo World/Movement/Jump")]
    public sealed class JumpMovementDefinition : MovementDefinition
    {
        [SerializeField, Min(0.01f)] private float _distance = 2f;
        [SerializeField, Min(0.01f)] private float _height = 0.6f;
        [SerializeField, Min(0.01f)] private float _jumpInterval = 2f;

        public float Distance => _distance;
        public float Height => _height;
        public float JumpInterval => _jumpInterval;

        public override void Validate()
        {
            RequirePositive(_distance, nameof(Distance));
            RequirePositive(_height, nameof(Height));
            RequirePositive(_jumpInterval, nameof(JumpInterval));

            throw new NotSupportedException(
                $"Movement '{name}': jumping will be implemented in stage 3. Remove this animal from Spawn Settings for now.");
        }

        public override IAnimalMovement CreateMovement(Rigidbody body, WorldBoundsProvider bounds, float radius)
        {
            throw new NotSupportedException("Jump movement will be implemented in stage 3.");
        }
    }
}
