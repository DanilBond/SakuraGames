using UnityEngine;

namespace ZooWorld.Animals.Movement
{
    public interface IAnimalMovement
    {
        void Reset(Vector3 direction);
        void FixedTick(float deltaTime);
        void OnCollision();
        void OnObstacleContact(Vector3 normal);
    }
}
