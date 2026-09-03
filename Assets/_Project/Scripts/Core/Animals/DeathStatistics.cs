using System;

namespace ZooWorld.Core.Animals
{
    public sealed class DeathStatistics
    {
        public int PreyDeaths { get; private set; }
        public int PredatorDeaths { get; private set; }

        public void RecordDeath(FoodRole role)
        {
            switch (role)
            {
                case FoodRole.Prey:
                    PreyDeaths++;
                    break;
                case FoodRole.Predator:
                    PredatorDeaths++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown food role.");
            }
        }
    }
}
