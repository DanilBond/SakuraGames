using UnityEngine;
using VContainer.Unity;
using ZooWorld.Spawning;
using ZooWorld.World;

namespace ZooWorld.Composition
{
    public sealed class GameStartup : IStartable
    {
        private readonly SpawnSettings _spawnSettings;
        private readonly WorldBoundsProvider _bounds;
        private readonly AnimalFactory _factory;
        private readonly SpawnService _spawnService;

        public GameStartup(SpawnSettings spawnSettings, WorldBoundsProvider bounds,
            AnimalFactory factory, SpawnService spawnService)
        {
            _spawnSettings = spawnSettings;
            _bounds = bounds;
            _factory = factory;
            _spawnService = spawnService;
        }

        public void Start()
        {
            _spawnSettings.Validate();
            _bounds.Initialize();
            _factory.Initialize(_spawnSettings);
            _spawnService.StartSpawning();

            Debug.Log($"[ZooWorld] Initialization complete. Animal definitions: {_spawnSettings.AnimalCount}.");
        }
    }
}
