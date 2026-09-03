using System;
using UnityEngine;
using ZooWorld.Animals.Movement;
using ZooWorld.World;

namespace ZooWorld.Animals.Definitions
{
    public abstract class MovementDefinition : ScriptableObject
    {
        public abstract void Validate();

        public abstract IAnimalMovement CreateMovement(Rigidbody body, WorldBoundsProvider bounds, float radius);

        public virtual void ValidateBody(Rigidbody body)
        {
        }

        protected void RequirePositive(float value, string fieldName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value <= 0f)
            {
                throw new InvalidOperationException(
                    $"Movement '{name}': {fieldName} must be a finite number greater than zero.");
            }
        }
    }
}
