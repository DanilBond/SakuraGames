using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core.Animals;
using ZooWorld.Spawning;

namespace ZooWorld.Composition
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Zoo World/Game Lifetime Scope")]
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private SpawnSettings _spawnSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_spawnSettings == null)
            {
                throw new InvalidOperationException(
                    "GameLifetimeScope: assign Spawn Settings in the Inspector.");
            }

            builder.RegisterInstance(_spawnSettings);
            builder.Register<AnimalIdProvider>(Lifetime.Scoped);
            builder.RegisterEntryPoint<GameStartup>(Lifetime.Scoped);
        }
    }
}
