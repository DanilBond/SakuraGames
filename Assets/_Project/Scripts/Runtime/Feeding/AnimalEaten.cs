using UnityEngine;
using ZooWorld.Animals;
using ZooWorld.Core.Animals;

namespace ZooWorld.Feeding
{
    public readonly struct AnimalEaten
    {
        public AnimalBehaviour Eater { get; }
        public long EaterSpawnId { get; }
        public Vector3 EaterPosition { get; }
        public long VictimSpawnId { get; }
        public string VictimSpeciesId { get; }
        public FoodRole VictimFoodRole { get; }

        public AnimalEaten(AnimalBehaviour eater, AnimalBehaviour victim)
        {
            Eater = eater;
            EaterSpawnId = eater.SpawnId;
            EaterPosition = eater.Position;
            VictimSpawnId = victim.SpawnId;
            VictimSpeciesId = victim.State.SpeciesId;
            VictimFoodRole = victim.State.FoodRole;
        }
    }
}
