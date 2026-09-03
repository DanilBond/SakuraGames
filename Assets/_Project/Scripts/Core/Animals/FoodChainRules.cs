namespace ZooWorld.Core.Animals
{
    public sealed class FoodChainRules
    {
        public bool TryEat(AnimalState first, AnimalState second, out AnimalState victim)
        {
            victim = null;

            if (first == null || second == null || !first.IsAlive || !second.IsAlive ||
                first.SpawnId == second.SpawnId)
            {
                return false;
            }

            bool firstPredator = first.FoodRole == FoodRole.Predator;
            bool secondPredator = second.FoodRole == FoodRole.Predator;

            if (!firstPredator && !secondPredator)
                return false;

            if (firstPredator && secondPredator)
            {
                // The older predator wins, regardless of which collision callback arrives first.
                victim = first.SpawnId > second.SpawnId ? first : second;
            }
            else
            {
                victim = firstPredator ? second : first;
            }

            return victim.TryKill();
        }
    }
}
