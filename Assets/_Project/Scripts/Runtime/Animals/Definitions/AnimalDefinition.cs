using System;
using UnityEngine;
using ZooWorld.Core.Animals;

namespace ZooWorld.Animals.Definitions
{
    [CreateAssetMenu(fileName = "Animal", menuName = "Zoo World/Animal")]
    public sealed class AnimalDefinition : ScriptableObject
    {
        [SerializeField] private string _speciesId;
        [SerializeField] private FoodRole _foodRole;
        [SerializeField] private MovementDefinition _movement;
        [SerializeField] private AnimalBehaviour _prefab;

        public string SpeciesId => _speciesId;
        public FoodRole FoodRole => _foodRole;
        public MovementDefinition Movement => _movement;
        public AnimalBehaviour Prefab => _prefab;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(_speciesId))
            {
                throw new InvalidOperationException($"Animal '{name}': Species Id is required.");
            }

            if (_speciesId != _speciesId.Trim())
            {
                throw new InvalidOperationException(
                    $"Animal '{name}': Species Id must not start or end with whitespace.");
            }

            if (_foodRole != FoodRole.Prey && _foodRole != FoodRole.Predator)
            {
                throw new InvalidOperationException($"Animal '{name}': unsupported Food Role.");
            }

            if (_movement == null)
            {
                throw new InvalidOperationException($"Animal '{name}': assign Movement.");
            }

            _movement.Validate();

            if (_prefab == null)
            {
                throw new InvalidOperationException($"Animal '{name}': assign Prefab.");
            }

            _prefab.ValidatePrefab();
        }
    }
}
