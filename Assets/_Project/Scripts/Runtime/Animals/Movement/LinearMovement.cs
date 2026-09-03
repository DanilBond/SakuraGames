using UnityEngine;
using ZooWorld.Animals.Definitions;
using ZooWorld.World;

namespace ZooWorld.Animals.Movement
{
    public sealed class LinearMovement : IAnimalMovement
    {
        private readonly Rigidbody _body;
        private readonly WorldBoundsProvider _bounds;
        private readonly float _speed;
        private readonly float _directionChangeInterval;
        private readonly float _collisionRecoveryTime;
        private readonly float _radius;
        private readonly ObstacleAvoidance _obstacles = new ObstacleAvoidance();

        private Vector3 _direction;
        private float _directionTimer;
        private float _recoveryTimer;
        private float _obstacleAvoidanceTimer;

        public LinearMovement(Rigidbody body, WorldBoundsProvider bounds, LinearMovementDefinition definition,
            float radius)
        {
            _body = body;
            _bounds = bounds;
            _speed = definition.Speed;
            _directionChangeInterval = definition.DirectionChangeInterval;
            _collisionRecoveryTime = definition.CollisionRecoveryTime;
            _radius = radius;
        }

        public void Reset(Vector3 direction)
        {
            direction.y = 0f;
            _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            _directionTimer = _directionChangeInterval;
            _recoveryTimer = 0f;
            _obstacleAvoidanceTimer = 0f;
            _obstacles.Clear();
        }

        public void FixedTick(float deltaTime)
        {
            _obstacleAvoidanceTimer -= deltaTime;

            if (_recoveryTimer > 0f && !_obstacles.HasContacts)
            {
                _recoveryTimer -= deltaTime;
                return;
            }

            _directionTimer -= deltaTime;

            if (_obstacleAvoidanceTimer <= 0f || _direction.sqrMagnitude < 0.0001f)
            {
                if (_bounds.TryGetReturnDirection(_body.position, _radius, out Vector3 returnDirection))
                {
                    _direction = returnDirection;
                    _directionTimer = _directionChangeInterval;
                }
                else if (_directionTimer <= 0f || _direction.sqrMagnitude < 0.0001f)
                {
                    float angle = Random.Range(0f, Mathf.PI * 2f);
                    _direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                    _directionTimer = _directionChangeInterval;
                }
            }

            if (_obstacles.HasContacts)
            {
                _direction = _obstacles.ResolveDirection(_direction);
                _directionTimer = _directionChangeInterval;
                _recoveryTimer = 0f;
                _obstacles.Clear();
            }

            Vector3 velocity = _body.linearVelocity;
            velocity.x = _direction.x * _speed;
            velocity.z = _direction.z * _speed;
            _body.linearVelocity = velocity;

            if (_direction.sqrMagnitude > 0.0001f)
                _body.MoveRotation(Quaternion.LookRotation(_direction));
        }

        public void OnCollision()
        {
            // Let the physics impulse act before restoring the cruising velocity.
            _recoveryTimer = _collisionRecoveryTime;
        }

        public void OnObstacleContact(Vector3 normal)
        {
            if (_obstacles.AddContact(normal))
                _obstacleAvoidanceTimer = Mathf.Max(_collisionRecoveryTime, 0.1f);
        }
    }
}
