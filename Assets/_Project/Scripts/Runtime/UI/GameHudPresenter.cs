using System;
using DG.Tweening;
using UnityEngine;
using VContainer.Unity;
using ZooWorld.Core.Animals;
using ZooWorld.Feeding;

namespace ZooWorld.UI
{
    public sealed class GameHudPresenter : ILateTickable, IDisposable
    {
        private readonly FeedingService _feeding;
        private readonly DeathStatistics _statistics;
        private readonly DeathCountersView _counters;
        private readonly TastyPopupLayer _popups;
        private readonly Camera _camera;
        private bool _initialized;
        private bool _disposed;
        private bool _countersDirty;

        public GameHudPresenter(FeedingService feeding, DeathStatistics statistics,
            DeathCountersView counters, TastyPopupLayer popups, Camera camera)
        {
            _feeding = feeding;
            _statistics = statistics;
            _counters = counters;
            _popups = popups;
            _camera = camera;
        }

        public void Initialize()
        {
            if (_initialized || _disposed)
                throw new InvalidOperationException("Game HUD is already initialized or disposed.");

            DOTween.Init();
            _counters.Initialize();
            _counters.Show(_statistics.PreyDeaths, _statistics.PredatorDeaths);
            _popups.Initialize(_camera);
            _feeding.AnimalEaten += OnAnimalEaten;
            _initialized = true;
        }

        public void LateTick()
        {
            if (!_initialized)
                return;

            if (_countersDirty)
            {
                _counters.Show(_statistics.PreyDeaths, _statistics.PredatorDeaths);
                _countersDirty = false;
            }

            // Advance only our tweens here, keeping popup motion in sync with world tracking.
            float deltaTime = Time.deltaTime;
            _counters.Tick(deltaTime);
            _popups.Tick(deltaTime);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _initialized = false;
            _feeding.AnimalEaten -= OnAnimalEaten;

            if (_counters != null)
                _counters.DisposeAnimations();

            if (_popups != null)
                _popups.Clear();
        }

        private void OnAnimalEaten(AnimalEaten eaten)
        {
            _statistics.RecordDeath(eaten.VictimFoodRole);
            _countersDirty = true;
            _popups.Show(eaten);
        }
    }
}
