using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core.Animals;
using ZooWorld.Feeding;
using ZooWorld.Spawning;
using ZooWorld.UI;
using ZooWorld.World;

namespace ZooWorld.Composition
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Zoo World/Game Lifetime Scope")]
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private SpawnSettings _spawnSettings;
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Transform _animalsRoot;
        [SerializeField] private float _groundHeight;
        [SerializeField, Min(0f)] private float _boundaryPadding = 0.5f;
        [SerializeField] private DeathCountersView _deathCounters;
        [SerializeField] private TastyPopupLayer _tastyPopups;

        protected override void Configure(IContainerBuilder builder)
        {
            if (_spawnSettings == null)
            {
                throw new InvalidOperationException(
                    "GameLifetimeScope: assign Spawn Settings in the Inspector.");
            }

            if (_worldCamera == null || _animalsRoot == null)
                throw new InvalidOperationException("GameLifetimeScope: assign World Camera and Animals Root in the Inspector.");

            if (_deathCounters == null || _tastyPopups == null)
                throw new InvalidOperationException("GameLifetimeScope: assign Death Counters and Tasty Popups in the Inspector.");

            builder.RegisterInstance(_spawnSettings);
            builder.Register<AnimalIdProvider>(Lifetime.Scoped);
            builder.Register<FoodChainRules>(Lifetime.Scoped);
            builder.Register<DeathStatistics>(Lifetime.Scoped);
            builder.RegisterInstance(_worldCamera);
            builder.RegisterInstance(_deathCounters);
            builder.RegisterInstance(_tastyPopups);
            builder.RegisterEntryPoint<FeedingService>(Lifetime.Scoped).AsSelf();
            builder.Register(_ => new WorldBoundsProvider(_worldCamera, _groundHeight, _boundaryPadding),
                Lifetime.Scoped);
            builder.Register(container => new AnimalFactory(container.Resolve<AnimalIdProvider>(),
                container.Resolve<WorldBoundsProvider>(), _animalsRoot, container.Resolve<FeedingService>()), Lifetime.Scoped);
            builder.RegisterEntryPoint<SpawnService>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<GameHudPresenter>(Lifetime.Scoped).AsSelf();
            builder.RegisterEntryPoint<GameStartup>(Lifetime.Scoped);
        }
    }
}
