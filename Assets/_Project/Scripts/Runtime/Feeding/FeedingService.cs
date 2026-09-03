using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using ZooWorld.Animals;
using ZooWorld.Core.Animals;

namespace ZooWorld.Feeding
{
    public sealed class FeedingService : ILateTickable, IDisposable
    {
        private readonly FoodChainRules _rules;
        private readonly Dictionary<Rigidbody, AnimalBehaviour> _animals = new Dictionary<Rigidbody, AnimalBehaviour>(128);
        private readonly List<PendingDespawn> _pendingDespawns = new List<PendingDespawn>(128);
        private bool _disposed;

        public event Action<AnimalEaten> AnimalEaten;

        public FeedingService(FoodChainRules rules)
        {
            _rules = rules;
        }

        internal void Register(AnimalBehaviour animal, Rigidbody body)
        {
            _animals.Add(body, animal);

            // Reserve during pool creation, so a crowded collision does not grow the queue.
            if (_pendingDespawns.Capacity < _animals.Count)
                _pendingDespawns.Capacity = Math.Max(_animals.Count, _pendingDespawns.Capacity * 2);
        }

        internal void Unregister(Rigidbody body)
        {
            if (!ReferenceEquals(body, null))
                _animals.Remove(body);
        }

        internal void HandleCollision(AnimalBehaviour first, Rigidbody otherBody)
        {
            if (_disposed || otherBody == null || !_animals.TryGetValue(otherBody, out AnimalBehaviour second) ||
                second == null || !_rules.TryEat(first.State, second.State, out AnimalState victimState))
            {
                return;
            }

            AnimalBehaviour victim = victimState == first.State ? first : second;
            AnimalBehaviour eater = victim == first ? second : first;
            var eaten = new AnimalEaten(eater, victim);

            // Hide now, but keep the instance out of the pool until collision callbacks are finished.
            _pendingDespawns.Add(new PendingDespawn(victim, eaten.VictimSpawnId));
            victim.HideAfterDeath();
            AnimalEaten?.Invoke(eaten);
        }

        public void LateTick()
        {
            for (int i = 0; i < _pendingDespawns.Count; i++)
            {
                PendingDespawn pending = _pendingDespawns[i];
                AnimalBehaviour animal = pending.Animal;

                if (animal != null && animal.SpawnId == pending.SpawnId && animal.IsSpawned && !animal.IsAlive)
                    animal.Owner.Despawn(animal);
            }

            _pendingDespawns.Clear();
        }

        public void Dispose()
        {
            _disposed = true;
            AnimalEaten = null;
            _pendingDespawns.Clear();
            _animals.Clear();
        }

        private readonly struct PendingDespawn
        {
            public AnimalBehaviour Animal { get; }
            public long SpawnId { get; }

            public PendingDespawn(AnimalBehaviour animal, long spawnId)
            {
                Animal = animal;
                SpawnId = spawnId;
            }
        }
    }
}
