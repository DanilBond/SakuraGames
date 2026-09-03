using NUnit.Framework;
using UnityEngine;
using ZooWorld.Animals.Movement;

namespace ZooWorld.Movement.Tests
{
    public sealed class ObstacleAvoidanceTests
    {
        [Test]
        public void WallRedirectsMovementButGroundAndClearedContactsDoNot()
        {
            var obstacles = new ObstacleAvoidance();
            Assert.That(obstacles.AddContact(Vector3.up), Is.False);
            Assert.That(obstacles.ResolveDirection(Vector3.right), Is.EqualTo(Vector3.right));

            obstacles.AddContact(Vector3.left);

            Assert.That(obstacles.ResolveDirection(Vector3.right), Is.EqualTo(Vector3.left));

            obstacles.Clear();

            Assert.That(obstacles.ResolveDirection(Vector3.right), Is.EqualTo(Vector3.right));
        }

        [Test]
        public void NarrowCornerDoesNotRedirectMovementIntoEitherWall()
        {
            var obstacles = new ObstacleAvoidance();
            Vector3 firstWall = Vector3.left;
            var secondWall = new Vector3(0.5f, 0f, -0.8660254f);
            obstacles.AddContact(firstWall);
            obstacles.AddContact(secondWall);

            Vector3 direction = obstacles.ResolveDirection(new Vector3(1f, 0f, 1f).normalized);

            Assert.That(direction.sqrMagnitude, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(Vector3.Dot(direction, firstWall), Is.GreaterThanOrEqualTo(-0.0001f));
            Assert.That(Vector3.Dot(direction, secondWall), Is.GreaterThanOrEqualTo(-0.0001f));
        }
    }
}
