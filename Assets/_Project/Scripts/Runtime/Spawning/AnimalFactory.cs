using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Animals.Definitions;
using ZooWorld.Core.Animals;
using ZooWorld.Feeding;
using ZooWorld.World;

namespace ZooWorld.Spawning
{
    public sealed class AnimalFactory : IDisposable
    {
        private readonly AnimalIdProvider _ids;
        private readonly WorldBoundsProvider _bounds;
        private readonly Transform _animalsRoot;
        private readonly FeedingService _feeding;

        private Dictionary<AnimalDefinition, AnimalPool> _pools;
        private Transform _stagingRoot;
        private bool _disposed;

        public AnimalFactory(AnimalIdProvider ids, WorldBoundsProvider bounds, Transform animalsRoot,
            FeedingService feeding)
        {
            _ids = ids;
            _bounds = bounds;
            _animalsRoot = animalsRoot;
            _feeding = feeding;
        }

        public void Initialize(SpawnSettings settings)
        {
            if (_disposed || _pools != null)
                throw new InvalidOperationException("Animal factory has already been initialized or disposed.");

            if (_animalsRoot == null || !_animalsRoot.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Animal factory: assign an active Animals Root in the scene.");

            if ((_animalsRoot.lossyScale - Vector3.one).sqrMagnitude > 0.000001f)
                throw new InvalidOperationException("Animal factory: Animals Root and its parents must have Scale (1, 1, 1).");

            for (int i = 0; i < settings.AnimalCount; i++)
                _bounds.ValidateRadius(settings.GetAnimal(i).Prefab.ValidatePrefab());

            _pools = new Dictionary<AnimalDefinition, AnimalPool>(settings.AnimalCount);
            var stagingObject = new GameObject("PoolStaging");
            stagingObject.SetActive(false);
            _stagingRoot = stagingObject.transform;
            _stagingRoot.SetParent(_animalsRoot, false);

            try
            {
                for (int i = 0; i < settings.AnimalCount; i++)
                {
                    AnimalDefinition definition = settings.GetAnimal(i);
                    var pool = new AnimalPool(this, definition, settings.PrewarmCountPerSpecies,
                        definition.Prefab.ValidatePrefab());
                    _pools.Add(definition, pool);
                    pool.Prewarm();
                }
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public float GetRadius(AnimalDefinition definition)
        {
            return _pools[definition].Radius;
        }

        public AnimalBehaviour Spawn(AnimalDefinition definition, Vector3 position, Vector3 direction)
        {
            AnimalPool pool = _pools[definition];
            AnimalBehaviour animal = pool.Get();

            try
            {
                animal.Spawn(_ids.Next(), position, direction);
                return animal;
            }
            catch
            {
                pool.Release(animal);
                throw;
            }
        }

        public void Despawn(AnimalBehaviour animal)
        {
            if (_disposed || animal == null || !animal.IsSpawned)
                return;

            if (animal.Owner != this)
                throw new InvalidOperationException("Cannot return an animal to a different factory.");

            _pools[animal.Definition].Release(animal);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_pools != null)
            {
                foreach (AnimalPool pool in _pools.Values)
                    pool.Dispose();

                _pools.Clear();
            }

            if (_stagingRoot != null)
                UnityEngine.Object.Destroy(_stagingRoot.gameObject);
        }

        internal AnimalBehaviour CreateInstance(AnimalDefinition definition)
        {
            // The inactive parent prevents callbacks before the instance has its dependencies.
            AnimalBehaviour animal = UnityEngine.Object.Instantiate(definition.Prefab, _stagingRoot);
            animal.gameObject.SetActive(false);

            try
            {
                animal.Initialize(definition, _bounds, this, _feeding);
                animal.transform.SetParent(_animalsRoot, false);
                return animal;
            }
            catch
            {
                UnityEngine.Object.Destroy(animal.gameObject);
                throw;
            }
        }
    }
}
