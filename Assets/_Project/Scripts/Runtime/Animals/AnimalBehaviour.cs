using System;
using UnityEngine;
using ZooWorld.Animals.Definitions;
using ZooWorld.Animals.Movement;
using ZooWorld.Core.Animals;
using ZooWorld.Feeding;
using ZooWorld.Spawning;
using ZooWorld.World;

namespace ZooWorld.Animals
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class AnimalBehaviour : MonoBehaviour
    {
        private Rigidbody _body;
        private SphereCollider _collider;
        private Transform _transform;
        private GameObject _gameObject;
        private IAnimalMovement _movement;
        private FeedingService _feeding;
        private float _collisionRadius;

        public AnimalDefinition Definition { get; private set; }
        public long SpawnId => State.SpawnId;
        public bool IsSpawned => State.LifeState != AnimalLifeState.Inactive;
        public bool IsAlive => State.IsAlive;
        public float CollisionRadius => _collisionRadius;
        public Vector3 Position => _body.position;

        internal AnimalState State { get; } = new AnimalState();
        internal AnimalFactory Owner { get; private set; }

        private void Awake()
        {
            EnsureComponents();
        }

        private void FixedUpdate()
        {
            if (IsAlive)
            {
                _movement.FixedTick(Time.fixedDeltaTime);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!IsAlive)
                return;

            Rigidbody otherBody = collision.rigidbody;

            if (otherBody == null || otherBody.isKinematic)
            {
                HandleObstacleContacts(collision);
            }
            else
            {
                _feeding.HandleCollision(this, otherBody);

                if (IsAlive)
                    _movement.OnCollision();
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (IsAlive && (collision.rigidbody == null || collision.rigidbody.isKinematic))
                HandleObstacleContacts(collision);
        }

        private void HandleObstacleContacts(Collision collision)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                _movement.OnObstacleContact(collision.GetContact(i).normal);
            }
        }

        private void OnDestroy()
        {
            _feeding?.Unregister(_body);
        }

        public void Initialize(AnimalDefinition definition, WorldBoundsProvider bounds, AnimalFactory owner,
            FeedingService feeding)
        {
            if (Definition != null)
            {
                throw new InvalidOperationException("Animal is already initialized.");
            }

            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (bounds == null)
            {
                throw new ArgumentNullException(nameof(bounds));
            }

            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (feeding == null)
                throw new ArgumentNullException(nameof(feeding));

            // Awake may not have run on an instance created under the inactive pool root.
            EnsureComponents();
            _collisionRadius = ValidatePrefab();
            ValidateMovement(definition.Movement);
            _movement = definition.Movement.CreateMovement(_body, bounds, _collisionRadius);
            Definition = definition;
            Owner = owner;
            _feeding = feeding;
            Despawn();
            _feeding.Register(this, _body);
        }

        internal void HideAfterDeath()
        {
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _gameObject.SetActive(false);
        }

        public void Spawn(long spawnId, Vector3 position, Vector3 direction)
        {
            if (Definition == null)
            {
                throw new InvalidOperationException("Initialize the animal before spawning it.");
            }

            State.Spawn(spawnId, Definition.SpeciesId, Definition.FoodRole);

            direction.y = 0f;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.forward;
            Quaternion rotation = Quaternion.LookRotation(direction);
            _movement.Reset(direction);
            _transform.SetPositionAndRotation(position, rotation);
            _gameObject.SetActive(true);
            _body.position = position;
            _body.rotation = rotation;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _body.WakeUp();
        }

        public void Despawn()
        {
            if (Definition == null)
            {
                throw new InvalidOperationException("Initialize the animal before despawning it.");
            }

            State.Despawn();
            _movement.Reset(Vector3.forward);
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            _gameObject.SetActive(false);
        }

        public float ValidatePrefab()
        {
            EnsureComponents();

            if (_body == null || _collider == null)
            {
                throw new InvalidOperationException(
                    $"Animal prefab '{name}': add Rigidbody and SphereCollider to the same object as AnimalBehaviour.");
            }

            if (_body.isKinematic || !_body.detectCollisions)
            {
                throw new InvalidOperationException(
                    $"Animal prefab '{name}': Rigidbody must be dynamic with Detect Collisions enabled.");
            }

            if (!_collider.enabled || _collider.isTrigger || _collider.center.sqrMagnitude > 0.000001f)
            {
                throw new InvalidOperationException(
                    $"Animal prefab '{name}': enable SphereCollider, disable Is Trigger and set Center to (0, 0, 0).");
            }

            if (GetComponentsInChildren<Collider>(true).Length != 1)
            {
                throw new InvalidOperationException(
                    $"Animal prefab '{name}': keep only the root SphereCollider; remove colliders from visual children.");
            }

            Vector3 scale = _transform.lossyScale;

            if (!IsPositiveFinite(scale.x) || !IsPositiveFinite(scale.y) || !IsPositiveFinite(scale.z) ||
                !Mathf.Approximately(scale.x, scale.y) || !Mathf.Approximately(scale.x, scale.z))
            {
                throw new InvalidOperationException(
                    $"Animal prefab '{name}': use a positive uniform root scale; resize the visual children separately.");
            }

            float radius = _collider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));

            if (!IsPositiveFinite(radius))
            {
                throw new InvalidOperationException($"Animal prefab '{name}': SphereCollider radius must be positive and finite.");
            }

            return radius;
        }

        public void ValidateMovement(MovementDefinition movement)
        {
            EnsureComponents();
            movement.ValidateBody(_body);
        }

        private void EnsureComponents()
        {
            if (_gameObject != null)
            {
                return;
            }

            _gameObject = gameObject;
            _transform = transform;
            _body = GetComponent<Rigidbody>();
            _collider = GetComponent<SphereCollider>();
        }

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
        }

        [ContextMenu("Return to pool")]
        private void ReturnToPool()
        {
            if (Application.isPlaying && IsSpawned)
            {
                Owner.Despawn(this);
            }
        }
    }
}
