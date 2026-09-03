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

            Vector3 gravity = Physics.gravity;

            if (float.IsNaN(gravity.y) || float.IsInfinity(gravity.y) || gravity.y >= -0.0001f ||
                gravity.x != 0f || gravity.z != 0f)
            {
                throw new InvalidOperationException(
                    $"Movement '{name}': jumping requires finite downward gravity on the Y axis.");
            }
        }

        public override void ValidateBody(Rigidbody body)
        {
            if (!body.useGravity || body.linearDamping != 0f ||
                (body.constraints & RigidbodyConstraints.FreezePosition) != 0)
            {
                throw new InvalidOperationException(
                    $"Movement '{name}', prefab '{body.name}': enable Use Gravity, set Linear Damping to 0 " +
                    "and uncheck all Freeze Position axes for jumping.");
            }
        }

        public override IAnimalMovement CreateMovement(Rigidbody body, WorldBoundsProvider bounds, float radius)
        {
            return new JumpMovement(body, bounds, this, radius);
        }
    }
}
