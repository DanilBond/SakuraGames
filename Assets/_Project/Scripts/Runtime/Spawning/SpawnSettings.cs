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
        [SerializeField, Min(1)] private int _prewarmCountPerSpecies = 64;
        [SerializeField] private LayerMask _animalLayers;
        [SerializeField, Min(0f)] private float _spawnClearance = 0.05f;

        public float MinInterval => _minInterval;
        public float MaxInterval => _maxInterval;
        public int AnimalCount => _animals?.Length ?? 0;
        public int PrewarmCountPerSpecies => _prewarmCountPerSpecies;
        public int AnimalLayers => _animalLayers.value;
        public float SpawnClearance => _spawnClearance;

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

            if (_prewarmCountPerSpecies < 1)
                throw new InvalidOperationException($"Spawn settings '{name}': Prewarm Count Per Species must be positive.");

            if (_animalLayers.value == 0)
                throw new InvalidOperationException($"Spawn settings '{name}': select the Animals layer in Animal Layers.");

            if (float.IsNaN(_spawnClearance) || float.IsInfinity(_spawnClearance) || _spawnClearance < 0f)
                throw new InvalidOperationException($"Spawn settings '{name}': Spawn Clearance must be finite and non-negative.");

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

                if ((_animalLayers.value & (1 << animal.Prefab.gameObject.layer)) == 0)
                {
                    throw new InvalidOperationException(
                        $"Animal '{animal.name}': the prefab's layer must be included in Spawn Settings / Animal Layers.");
                }

                if (!speciesIds.Add(animal.SpeciesId))
                {
                    throw new InvalidOperationException(
                        $"Spawn settings '{name}': duplicate Species Id '{animal.SpeciesId}'.");
                }
            }
        }
    }
}
