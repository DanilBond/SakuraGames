using System;
using UnityEngine;
using ZooWorld.Animals.Movement;
using ZooWorld.World;

namespace ZooWorld.Animals.Definitions
{
    [CreateAssetMenu(fileName = "LinearMovement", menuName = "Zoo World/Movement/Linear")]
    public sealed class LinearMovementDefinition : MovementDefinition
    {
        [SerializeField, Min(0.01f)] private float _speed = 2f;
        [SerializeField, Min(0.01f)] private float _directionChangeInterval = 2f;
        [SerializeField, Min(0f)] private float _collisionRecoveryTime = 0.25f;

        public float Speed => _speed;
        public float DirectionChangeInterval => _directionChangeInterval;
        public float CollisionRecoveryTime => _collisionRecoveryTime;

        public override void Validate()
        {
            RequirePositive(_speed, nameof(Speed));
            RequirePositive(_directionChangeInterval, nameof(DirectionChangeInterval));

            if (float.IsNaN(_collisionRecoveryTime) || float.IsInfinity(_collisionRecoveryTime) ||
                _collisionRecoveryTime < 0f)
            {
                throw new InvalidOperationException(
                    $"Movement '{name}': Collision Recovery Time must be a finite number greater than or equal to zero.");
            }
        }

        public override IAnimalMovement CreateMovement(Rigidbody body, WorldBoundsProvider bounds, float radius)
        {
            return new LinearMovement(body, bounds, this, radius);
        }
    }
}
