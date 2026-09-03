using UnityEngine;
using UnityEngine.SceneManagement;
using ZooWorld.Animals.Definitions;
using ZooWorld.World;

namespace ZooWorld.Animals.Movement
{
    public sealed class JumpMovement : IAnimalMovement
    {
        private const float CollisionRecoveryTime = 0.25f;
        private const float GroundTolerance = 0.03f;

        private readonly Rigidbody _body;
        private readonly WorldBoundsProvider _bounds;
        private readonly PhysicsScene _physicsScene;
        private readonly ObstacleAvoidance _obstacles = new ObstacleAvoidance();
        private readonly float _radius;
        private readonly float _jumpInterval;
        private readonly float _verticalSpeed;
        private readonly float _horizontalSpeed;
        private readonly float _gravity;
        private readonly int _collisionLayers;

        private Vector3 _direction;
        private float _waitTimer;
        private float _recoveryTimer;
        private float _avoidanceTimer;
        private bool _airborne;

        public JumpMovement(Rigidbody body, WorldBoundsProvider bounds, JumpMovementDefinition definition,
            float radius)
        {
            _body = body;
            _bounds = bounds;
            _physicsScene = body.gameObject.scene.GetPhysicsScene();
            _radius = radius;
            _jumpInterval = definition.JumpInterval;
            _gravity = -Physics.gravity.y;

            // h = v² / (2g), flight time = 2v / g on level ground.
            _verticalSpeed = Mathf.Sqrt(2f * _gravity * definition.Height);
            _horizontalSpeed = definition.Distance * _gravity / (2f * _verticalSpeed);

            int animalLayer = body.gameObject.layer;

            for (int layer = 0; layer < 32; layer++)
            {
                if (!Physics.GetIgnoreLayerCollision(animalLayer, layer))
                    _collisionLayers |= 1 << layer;
            }
        }

        public void Reset(Vector3 direction)
        {
            direction.y = 0f;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            _waitTimer = _jumpInterval;
            _recoveryTimer = 0f;
            _avoidanceTimer = 0f;
            _airborne = false;
            _obstacles.Clear();
        }

        public void FixedTick(float deltaTime)
        {
            _recoveryTimer -= deltaTime;
            _avoidanceTimer -= deltaTime;
            Vector3 velocity = _body.linearVelocity;
            bool grounded = IsGrounded(velocity);

            if (!grounded)
            {
                _airborne = true;
                SteerInAir(ref velocity);
                _obstacles.Clear();
                return;
            }

            if (_airborne)
            {
                _airborne = false;
                _waitTimer = _jumpInterval;
            }

            _waitTimer -= deltaTime;

            if (_recoveryTimer <= 0f)
            {
                velocity.x = 0f;
                velocity.z = 0f;
                _body.linearVelocity = velocity;

                if (_waitTimer <= 0f)
                    Jump(deltaTime);
            }

            _obstacles.Clear();
        }

        public void OnCollision()
        {
            _recoveryTimer = CollisionRecoveryTime;
        }

        public void OnObstacleContact(Vector3 normal)
        {
            if (_obstacles.AddContact(normal))
                _avoidanceTimer = CollisionRecoveryTime;
        }

        private bool IsGrounded(Vector3 velocity)
        {
            if (velocity.y > 0.1f)
                return false;

            // A slightly smaller sphere starts clear of the floor and ignores our own collider.
            float probeRadius = _radius * 0.9f;
            return _physicsScene.SphereCast(_body.position, probeRadius, Vector3.down, out RaycastHit hit,
                       _radius - probeRadius + GroundTolerance, _collisionLayers, QueryTriggerInteraction.Ignore) &&
                   hit.normal.y >= 0.7f;
        }

        private void Jump(float deltaTime)
        {
            if (!_bounds.TryGetReturnDirection(_body.position, _radius, out _direction))
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                _direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
            }

            _direction = _obstacles.ResolveDirection(_direction);

            if (_direction.sqrMagnitude < 0.0001f)
                return;

            Vector3 velocity = _direction * _horizontalSpeed;
            // PhysX applies gravity before moving the body; compensate half a fixed step.
            velocity.y = _verticalSpeed + _gravity * deltaTime * 0.5f;
            _body.linearVelocity = velocity;
            _body.MoveRotation(Quaternion.LookRotation(_direction));
            _airborne = true;
        }

        private void SteerInAir(ref Vector3 velocity)
        {
            if (_obstacles.HasContacts)
            {
                Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
                Vector3 incoming = horizontalVelocity.sqrMagnitude > 0.0001f
                    ? horizontalVelocity.normalized
                    : _direction;
                _direction = _obstacles.ResolveDirection(incoming);
            }
            else
            {
                if (_avoidanceTimer > 0f || _recoveryTimer > 0f ||
                    !_bounds.TryGetReturnDirection(_body.position, _radius, out Vector3 returnDirection))
                {
                    return;
                }

                _direction = returnDirection;
            }

            velocity.x = _direction.x * _horizontalSpeed;
            velocity.z = _direction.z * _horizontalSpeed;
            _body.linearVelocity = velocity;

            if (_direction.sqrMagnitude > 0.0001f)
                _body.MoveRotation(Quaternion.LookRotation(_direction));
        }
    }
}
