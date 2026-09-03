using UnityEngine;

namespace ZooWorld.Animals.Definitions
{
    [CreateAssetMenu(fileName = "LinearMovement", menuName = "Zoo World/Movement/Linear")]
    public sealed class LinearMovementDefinition : MovementDefinition
    {
        [SerializeField, Min(0.01f)] private float _speed = 2f;
        [SerializeField, Min(0.01f)] private float _directionChangeInterval = 2f;

        public float Speed => _speed;
        public float DirectionChangeInterval => _directionChangeInterval;

        public override void Validate()
        {
            RequirePositive(_speed, nameof(Speed));
            RequirePositive(_directionChangeInterval, nameof(DirectionChangeInterval));
        }
    }
}
