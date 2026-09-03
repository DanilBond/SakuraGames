using System;
using UnityEngine;

namespace ZooWorld.Animals.Definitions
{
    public abstract class MovementDefinition : ScriptableObject
    {
        public abstract void Validate();

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
