using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using ZooWorld.Animals;
using ZooWorld.Animals.Definitions;

namespace ZooWorld.Spawning
{
    internal sealed class AnimalPool : IDisposable
    {
        private readonly AnimalFactory _factory;
        private readonly AnimalDefinition _definition;
        private readonly List<AnimalBehaviour> _instances;
        private readonly ObjectPool<AnimalBehaviour> _pool;
        private readonly int _initialSize;

        public float Radius { get; }

        public AnimalPool(AnimalFactory factory, AnimalDefinition definition, int initialSize, float radius)
        {
            _factory = factory;
            _definition = definition;
            _initialSize = initialSize;
            Radius = radius;
            _instances = new List<AnimalBehaviour>(initialSize);
            _pool = new ObjectPool<AnimalBehaviour>(CreateInstance,
                collectionCheck: true, defaultCapacity: initialSize, maxSize: int.MaxValue);
        }

        public void Prewarm()
        {
            // Keep all instances checked out until the pool has reached the requested size.
            for (int i = 0; i < _initialSize; i++)
                _pool.Get();

            for (int i = 0; i < _initialSize; i++)
                _pool.Release(_instances[i]);
        }

        public AnimalBehaviour Get()
        {
            return _pool.Get();
        }

        public void Release(AnimalBehaviour animal)
        {
            animal.Despawn();
            _pool.Release(animal);
        }

        public void Dispose()
        {
            _pool.Dispose();

            // The pool itself only knows about idle objects; this list also owns active animals.
            for (int i = 0; i < _instances.Count; i++)
            {
                if (_instances[i] != null)
                    UnityEngine.Object.Destroy(_instances[i].gameObject);
            }

            _instances.Clear();
        }

        private AnimalBehaviour CreateInstance()
        {
            AnimalBehaviour animal = _factory.CreateInstance(_definition);
            _instances.Add(animal);
            return animal;
        }
    }
}
