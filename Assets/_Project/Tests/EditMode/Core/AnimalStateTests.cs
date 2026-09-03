using System;
using NUnit.Framework;
using ZooWorld.Core.Animals;

namespace ZooWorld.Core.Tests
{
    public sealed class AnimalStateTests
    {
        [Test]
        public void DeathCanBeRegisteredOnlyOncePerSpawn()
        {
            var animal = new AnimalState();
            animal.Spawn(1, "frog", FoodRole.Prey);

            Assert.That(animal.TryKill(), Is.True);
            Assert.That(animal.TryKill(), Is.False);
            Assert.That(animal.LifeState, Is.EqualTo(AnimalLifeState.Dead));
            Assert.That(animal.IsAlive, Is.False);
            Assert.That(animal.SpawnId, Is.EqualTo(1));
            Assert.That(animal.SpeciesId, Is.EqualTo("frog"));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SpawnWithoutDespawnIsRejectedWithoutChangingIdentity(bool killFirst)
        {
            var animal = new AnimalState();
            animal.Spawn(1, "snake", FoodRole.Predator);

            if (killFirst)
            {
                animal.TryKill();
            }

            Assert.Throws<InvalidOperationException>(() => animal.Spawn(2, "frog", FoodRole.Prey));
            Assert.That(animal.SpawnId, Is.EqualTo(1));
            Assert.That(animal.SpeciesId, Is.EqualTo("snake"));
            Assert.That(animal.FoodRole, Is.EqualTo(FoodRole.Predator));
            Assert.That(animal.LifeState, Is.EqualTo(killFirst ? AnimalLifeState.Dead : AnimalLifeState.Alive));
        }

        [Test]
        public void DespawnClearsIdentityAndAllowsReuse()
        {
            var ids = new AnimalIdProvider();
            var animal = new AnimalState();
            long firstId = ids.Next();
            animal.Spawn(firstId, "snake", FoodRole.Predator);

            animal.TryKill();
            animal.Despawn();

            Assert.That(animal.LifeState, Is.EqualTo(AnimalLifeState.Inactive));
            Assert.That(animal.SpawnId, Is.Zero);
            Assert.That(animal.SpeciesId, Is.Null);
            Assert.That(animal.TryKill(), Is.False);

            animal.Spawn(ids.Next(), "frog", FoodRole.Prey);

            Assert.That(animal.SpawnId, Is.GreaterThan(firstId));
            Assert.That(animal.SpeciesId, Is.EqualTo("frog"));
            Assert.That(animal.FoodRole, Is.EqualTo(FoodRole.Prey));
            Assert.That(animal.IsAlive, Is.True);
            Assert.That(animal.TryKill(), Is.True);
        }
    }
}
