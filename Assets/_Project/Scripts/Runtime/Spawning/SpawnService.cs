using System;
using UnityEngine;
using VContainer.Unity;
using ZooWorld.Animals.Definitions;
using ZooWorld.World;

namespace ZooWorld.Spawning
{
    public sealed class SpawnService : ITickable, IDisposable
    {
        private const int PositionAttempts = 16;
        private const float RetryDelay = 0.1f;
        private const float GroundClearance = 0.02f;

        private readonly SpawnSettings _settings;
        private readonly AnimalFactory _factory;
        private readonly WorldBoundsProvider _bounds;

        private AnimalDefinition[] _definitions;
        private int[] _obstacleLayers;
        private AnimalDefinition _nextAnimal;
        private float _timeRemaining;
        private float _minInterval;
        private float _maxInterval;
        private float _spawnClearance;
        private int _animalLayers;
        private int _nextObstacleLayers;
        private bool _running;

        public SpawnService(SpawnSettings settings, AnimalFactory factory, WorldBoundsProvider bounds)
        {
            _settings = settings;
            _factory = factory;
            _bounds = bounds;
        }

        public void StartSpawning()
        {
            if (_running)
                throw new InvalidOperationException("Spawning has already started.");

            _definitions = new AnimalDefinition[_settings.AnimalCount];
            _obstacleLayers = new int[_definitions.Length];
            _animalLayers = _settings.AnimalLayers;

            for (int i = 0; i < _definitions.Length; i++)
            {
                _definitions[i] = _settings.GetAnimal(i);
                int animalLayer = _definitions[i].Prefab.gameObject.layer;

                for (int layer = 0; layer < 32; layer++)
                {
                    if (!Physics.GetIgnoreLayerCollision(animalLayer, layer))
                        _obstacleLayers[i] |= 1 << layer;
                }

                _obstacleLayers[i] &= ~_animalLayers;
            }

            _minInterval = _settings.MinInterval;
            _maxInterval = _settings.MaxInterval;
            _spawnClearance = _settings.SpawnClearance;
            ScheduleNextSpawn();
            _running = true;
        }

        public void Tick()
        {
            if (!_running)
                return;

            _bounds.Refresh();
            _timeRemaining -= Time.deltaTime;

            if (_timeRemaining > 0f)
                return;

            if (!TryFindPosition(_factory.GetRadius(_nextAnimal), out Vector3 position))
            {
                _timeRemaining = RetryDelay;
                return;
            }

            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            _factory.Spawn(_nextAnimal, position, direction);
            ScheduleNextSpawn();
        }

        public void Dispose()
        {
            _running = false;
        }

        private void ScheduleNextSpawn()
        {
            int index = UnityEngine.Random.Range(0, _definitions.Length);
            _nextAnimal = _definitions[index];
            _nextObstacleLayers = _obstacleLayers[index];
            _timeRemaining = UnityEngine.Random.Range(_minInterval, _maxInterval);
        }

        private bool TryFindPosition(float radius, out Vector3 position)
        {
            position = default;

            for (int i = 0; i < PositionAttempts; i++)
            {
                if (!_bounds.TryGetRandomPosition(radius, out position))
                    return false;

                position.y = _bounds.GroundHeight + radius + GroundClearance;

                if (Physics.CheckSphere(position, radius + _spawnClearance,
                        _animalLayers, QueryTriggerInteraction.Ignore))
                    continue;

                // Extra clearance is only for animals; it would intersect the ground here.
                if (_nextObstacleLayers != 0 && Physics.CheckSphere(position, radius,
                        _nextObstacleLayers, QueryTriggerInteraction.Ignore))
                    continue;

                return true;
            }

            return false;
        }
    }
}
