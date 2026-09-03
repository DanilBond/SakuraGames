using UnityEngine;

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
        }
    }
}
