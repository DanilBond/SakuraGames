using NUnit.Framework;
using ZooWorld.Core.Animals;

namespace ZooWorld.Core.Tests
{
    public sealed class FoodChainRulesTests
    {
        [Test]
        public void PreyCollisionLeavesBothAlive()
        {
            var rules = new FoodChainRules();
            var first = Spawn(1, "frog", FoodRole.Prey);
            var second = Spawn(2, "rabbit", FoodRole.Prey);

            Assert.That(rules.TryEat(first, second, out AnimalState victim), Is.False);
            Assert.That(victim, Is.Null);
            Assert.That(first.IsAlive && second.IsAlive, Is.True);
        }

        [Test]
        public void PreyDiesOnceAndCanBeEatenAgainOnlyAfterRespawn()
        {
            var rules = new FoodChainRules();
            var prey = Spawn(1, "frog", FoodRole.Prey);
            var predator = Spawn(2, "snake", FoodRole.Predator);
            var otherPredator = Spawn(3, "snake", FoodRole.Predator);

            Assert.That(rules.TryEat(prey, predator, out AnimalState victim), Is.True);
            Assert.That(victim, Is.SameAs(prey));
            Assert.That(predator.IsAlive, Is.True);
            Assert.That(prey.IsAlive, Is.False);
            Assert.That(rules.TryEat(predator, prey, out _), Is.False);
            Assert.That(rules.TryEat(otherPredator, prey, out _), Is.False);

            prey.Despawn();
            Assert.That(rules.TryEat(predator, prey, out _), Is.False);
            prey.Spawn(4, "frog", FoodRole.Prey);

            Assert.That(rules.TryEat(predator, prey, out victim), Is.True);
            Assert.That(victim, Is.SameAs(prey));
        }

        [Test]
        public void OlderPredatorWinsInEitherCallbackOrder()
        {
            var rules = new FoodChainRules();
            var older = Spawn(10, "snake", FoodRole.Predator);
            var younger = Spawn(11, "snake", FoodRole.Predator);

            Assert.That(rules.TryEat(older, younger, out AnimalState victim), Is.True);
            Assert.That(victim, Is.SameAs(younger));
            Assert.That(older.IsAlive, Is.True);
            Assert.That(rules.TryEat(younger, older, out _), Is.False);

            older.Despawn();
            younger.Despawn();
            older.Spawn(12, "snake", FoodRole.Predator);
            younger.Spawn(13, "snake", FoodRole.Predator);

            Assert.That(rules.TryEat(younger, older, out victim), Is.True);
            Assert.That(victim, Is.SameAs(younger));
            Assert.That(older.IsAlive, Is.True);
        }

        private static AnimalState Spawn(long id, string speciesId, FoodRole role)
        {
            var animal = new AnimalState();
            animal.Spawn(id, speciesId, role);
            return animal;
        }
    }
}
