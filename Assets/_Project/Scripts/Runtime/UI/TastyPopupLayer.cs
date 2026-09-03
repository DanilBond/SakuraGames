using System;
using UnityEngine;
using ZooWorld.Feeding;

namespace ZooWorld.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Zoo World/UI/Tasty Popup Layer")]
    public sealed class TastyPopupLayer : MonoBehaviour
    {
        [SerializeField] private TastyPopupView _prefab;
        [SerializeField, Min(1)] private int _capacity = 32;
        [SerializeField, Min(0.01f)] private float _duration = 1f;
        [SerializeField] private Vector2 _offset = new Vector2(0f, -40f);
        [SerializeField, Min(0f)] private float _riseDistance = 16f;

        private RectTransform _rect;
        private Canvas _canvas;
        private TastyPopupView[] _items;
        private int _nextIndex;

        private void Awake()
        {
            EnsureComponents();
        }

        public void Initialize(Camera camera)
        {
            EnsureComponents();

            if (_items != null)
                throw new InvalidOperationException("Tasty popup layer is already initialized.");

            if (!isActiveAndEnabled || _canvas == null || !_canvas.isActiveAndEnabled ||
                _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
                throw new InvalidOperationException("Tasty popup layer: use an active Screen Space - Overlay Canvas.");

            if (camera == null || camera.targetDisplay != _canvas.targetDisplay)
                throw new InvalidOperationException("Tasty popup layer: the gameplay camera and Canvas must use the same Target Display.");

            if (_prefab == null || _prefab.gameObject.scene.IsValid())
                throw new InvalidOperationException("Tasty popup layer: assign a popup prefab from Project.");

            if (_capacity < 1 || !IsFinite(_duration) || _duration <= 0f ||
                !IsFinite(_riseDistance) || _riseDistance < 0f || !IsFinite(_offset.x) || !IsFinite(_offset.y))
            {
                throw new InvalidOperationException("Tasty popup layer: check Capacity, Duration, Offset and Rise Distance.");
            }

            _prefab.ValidatePrefab();
            _items = new TastyPopupView[_capacity];

            try
            {
                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i] = Instantiate(_prefab, _rect);
                    _items[i].Initialize(camera, _rect, _duration, _offset, _riseDistance);
                }
            }
            catch
            {
                Clear();
                throw;
            }
        }

        public void Show(AnimalEaten eaten)
        {
            int freeIndex = -1;

            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i].IsFor(eaten.Eater, eaten.EaterSpawnId))
                {
                    _items[i].Show(eaten.Eater, eaten.EaterSpawnId);
                    return;
                }

                if (!_items[i].IsActive && freeIndex < 0)
                    freeIndex = i;
            }

            int index = freeIndex >= 0 ? freeIndex : _nextIndex;
            _items[index].Show(eaten.Eater, eaten.EaterSpawnId);
            _nextIndex = (index + 1) % _items.Length;
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _items.Length; i++)
                _items[i].Tick(deltaTime);
        }

        public void Clear()
        {
            if (_items == null)
                return;

            for (int i = 0; i < _items.Length; i++)
            {
                if (_items[i] != null)
                    Destroy(_items[i].gameObject);
            }

            _items = null;
            _nextIndex = 0;
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void EnsureComponents()
        {
            if (_rect != null)
                return;

            _rect = GetComponent<RectTransform>();
            Canvas canvas = GetComponentInParent<Canvas>();
            _canvas = canvas != null ? canvas.rootCanvas : null;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
