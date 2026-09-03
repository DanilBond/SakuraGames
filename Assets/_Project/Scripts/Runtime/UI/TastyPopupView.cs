using System;
using TMPro;
using UnityEngine;
using ZooWorld.Animals;

namespace ZooWorld.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI), typeof(CanvasGroup))]
    [AddComponentMenu("Zoo World/UI/Tasty Popup View")]
    public sealed class TastyPopupView : MonoBehaviour
    {
        private RectTransform _rect;
        private TextMeshProUGUI _text;
        private CanvasGroup _group;
        private GameObject _gameObject;
        private Camera _camera;
        private RectTransform _root;
        private AnimalBehaviour _target;
        private long _spawnId;
        private float _elapsed;
        private float _duration;
        private float _riseDistance;
        private Vector2 _offset;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            EnsureComponents();
        }

        public void ValidatePrefab()
        {
            EnsureComponents();

            if (_text == null || _group == null || _rect == null || _text.font == null || !_text.enabled)
                throw new InvalidOperationException("Tasty popup: add an enabled TextMeshProUGUI with a font and a CanvasGroup on the root.");
        }

        public void Initialize(Camera camera, RectTransform root, float duration, Vector2 offset, float riseDistance)
        {
            ValidatePrefab();
            _camera = camera;
            _root = root;
            _duration = duration;
            _offset = offset;
            _riseDistance = riseDistance;
            _rect.localScale = Vector3.one;
            _rect.localRotation = Quaternion.identity;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _text.raycastTarget = false;
            _gameObject.SetActive(true);
            _text.SetText("Tasty!");
            _text.ForceMeshUpdate();
            Hide();
        }

        public bool IsFor(AnimalBehaviour animal, long spawnId)
        {
            return IsActive && _target == animal && _spawnId == spawnId;
        }

        public void Show(AnimalBehaviour target, long spawnId)
        {
            _target = target;
            _spawnId = spawnId;
            _elapsed = 0f;
            IsActive = true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
                return;

            _elapsed += deltaTime;

            if (_elapsed >= _duration || _target == null || !_target.IsAlive || _target.SpawnId != _spawnId)
            {
                Hide();
                return;
            }

            Vector3 screen = _camera.WorldToScreenPoint(_target.Position);
            bool visible = screen.z > 0f && _camera.pixelRect.Contains(new Vector2(screen.x, screen.y));

            if (_gameObject.activeSelf != visible)
                _gameObject.SetActive(visible);

            if (!visible)
                return;

            // Overlay Canvas coordinates are relative to the parent pivot, independent of CanvasScaler.
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, null, out Vector2 point))
            {
                float progress = _elapsed / _duration;
                point += _offset + Vector2.up * (_riseDistance * progress);
                _rect.localPosition = new Vector3(point.x, point.y, 0f);
                _group.alpha = 1f - progress;
            }
        }

        private void Hide()
        {
            IsActive = false;
            _target = null;
            _spawnId = 0;
            _elapsed = 0f;
            _group.alpha = 0f;
            _gameObject.SetActive(false);
        }

        private void EnsureComponents()
        {
            if (_gameObject != null)
                return;

            _gameObject = gameObject;
            _rect = GetComponent<RectTransform>();
            _text = GetComponent<TextMeshProUGUI>();
            _group = GetComponent<CanvasGroup>();
        }
    }
}
