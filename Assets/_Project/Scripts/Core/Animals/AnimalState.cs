using System;

namespace ZooWorld.Core.Animals
{
    public sealed class AnimalState
    {
        public long SpawnId { get; private set; }
        public string SpeciesId { get; private set; }
        public FoodRole FoodRole { get; private set; }
        public AnimalLifeState LifeState { get; private set; }
        public bool IsAlive => LifeState == AnimalLifeState.Alive;

        // A pooled instance gets a new ID from AnimalIdProvider on every spawn.
        public void Spawn(long spawnId, string speciesId, FoodRole foodRole)
        {
            if (LifeState != AnimalLifeState.Inactive)
            {
                throw new InvalidOperationException("Despawn the animal before spawning it again.");
            }

            if (spawnId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spawnId), "Spawn ID must be positive.");
            }

            if (string.IsNullOrWhiteSpace(speciesId))
            {
                throw new ArgumentException("Species ID must not be empty.", nameof(speciesId));
            }

            if (foodRole != FoodRole.Prey && foodRole != FoodRole.Predator)
            {
                throw new ArgumentOutOfRangeException(nameof(foodRole), "Unknown food role.");
            }

            SpawnId = spawnId;
            SpeciesId = speciesId;
            FoodRole = foodRole;
            LifeState = AnimalLifeState.Alive;
        }

        public bool TryKill()
        {
            if (!IsAlive)
            {
                return false;
            }

            LifeState = AnimalLifeState.Dead;
            return true;
        }

        public void Despawn()
        {
            LifeState = AnimalLifeState.Inactive;
            SpawnId = 0;
            SpeciesId = null;
            FoodRole = default;
        }
    }
}
