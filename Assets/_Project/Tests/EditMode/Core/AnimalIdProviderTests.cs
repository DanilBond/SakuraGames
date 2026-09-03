using NUnit.Framework;
using ZooWorld.Core.Animals;

namespace ZooWorld.Core.Tests
{
    public sealed class AnimalIdProviderTests
    {
        [Test]
        public void SessionsHaveIndependentIdSequences()
        {
            var firstSession = new AnimalIdProvider();
            Assert.That(firstSession.Next(), Is.EqualTo(1));
            Assert.That(firstSession.Next(), Is.EqualTo(2));

            var secondSession = new AnimalIdProvider();

            Assert.That(secondSession.Next(), Is.EqualTo(1));
            Assert.That(firstSession.Next(), Is.EqualTo(3));
        }
    }
}
