using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Animals.Definitions;

namespace ZooWorld.Spawning
{
    [CreateAssetMenu(fileName = "SpawnSettings", menuName = "Zoo World/Spawn Settings")]
    public sealed class SpawnSettings : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _minInterval = 1f;
        [SerializeField, Min(0.01f)] private float _maxInterval = 2f;
        [SerializeField] private AnimalDefinition[] _animals;

        public float MinInterval => _minInterval;
        public float MaxInterval => _maxInterval;
        public int AnimalCount => _animals?.Length ?? 0;

        public AnimalDefinition GetAnimal(int index)
        {
            return _animals[index];
        }

        public void Validate()
        {
            if (float.IsNaN(_minInterval) || float.IsInfinity(_minInterval) || _minInterval <= 0f ||
                float.IsNaN(_maxInterval) || float.IsInfinity(_maxInterval) || _maxInterval < _minInterval)
            {
                throw new InvalidOperationException(
                    $"Spawn settings '{name}': intervals must be finite and 0 < Min Interval <= Max Interval.");
            }

            if (AnimalCount == 0)
            {
                throw new InvalidOperationException($"Spawn settings '{name}': add at least one animal.");
            }

            var speciesIds = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < _animals.Length; i++)
            {
                AnimalDefinition animal = _animals[i];

                if (animal == null)
                {
                    throw new InvalidOperationException(
                        $"Spawn settings '{name}': Animals element {i} is empty.");
                }

                animal.Validate();

                if (!speciesIds.Add(animal.SpeciesId))
                {
                    throw new InvalidOperationException(
                        $"Spawn settings '{name}': duplicate Species Id '{animal.SpeciesId}'.");
                }
            }
        }
    }
}
