using UnityEngine;
using VContainer.Unity;
using ZooWorld.Spawning;

namespace ZooWorld.Composition
{
    public sealed class GameStartup : IStartable
    {
        private readonly SpawnSettings _spawnSettings;

        public GameStartup(SpawnSettings spawnSettings)
        {
            _spawnSettings = spawnSettings;
        }

        public void Start()
        {
            _spawnSettings.Validate();

            Debug.Log($"[ZooWorld] Initialization complete. Animal definitions: {_spawnSettings.AnimalCount}.");
        }
    }
}
