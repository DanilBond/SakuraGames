using System;
using DG.Tweening;
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
        private Sequence _animation;
        private float _riseOffset;
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
            if (_animation != null)
                throw new InvalidOperationException("Tasty popup is already initialized.");

            ValidatePrefab();
            _camera = camera;
            _root = root;
            _offset = offset;
            _rect.localScale = Vector3.one;
            _rect.localRotation = Quaternion.identity;
            _group.interactable = false;
            _group.blocksRaycasts = false;
            _text.raycastTarget = false;
            _gameObject.SetActive(true);
            _text.SetText("Tasty!");
            _text.ForceMeshUpdate();
            CreateAnimation(duration, riseDistance);
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
            IsActive = true;
            _animation.Restart();
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
                return;

            if (_target == null || !_target.IsAlive || _target.SpawnId != _spawnId)
            {
                Hide();
                return;
            }

            _animation.ManualUpdate(deltaTime, deltaTime);

            if (_animation.IsComplete())
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
                point += _offset + Vector2.up * _riseOffset;
                _rect.localPosition = new Vector3(point.x, point.y, 0f);
            }
        }

        public void DisposeAnimations()
        {
            _animation?.Kill();
            _animation = null;
            IsActive = false;
            _target = null;
            _spawnId = 0;
        }

        private void OnDestroy()
        {
            DisposeAnimations();
        }

        private void CreateAnimation(float duration, float riseDistance)
        {
            _animation = DOTween.Sequence()
                .SetAutoKill(false)
                .SetRecyclable(false)
                .SetUpdate(UpdateType.Manual)
                .Append(_rect.DOScale(Vector3.one, duration * 0.2f)
                    .From(Vector3.one * 0.75f, false).SetEase(Ease.OutBack))
                .Join(DOTween.To(() => _group.alpha, value => _group.alpha = value, 1f, duration * 0.12f)
                    .From(0f, false).SetEase(Ease.OutQuad))
                .Insert(0f, DOTween.To(() => _riseOffset, value => _riseOffset = value, riseDistance, duration)
                    .From(0f, false).SetEase(Ease.OutQuad))
                .Insert(duration * 0.7f, DOTween.To(() => _group.alpha, value => _group.alpha = value, 0f, duration * 0.3f)
                    .From(1f, false).SetEase(Ease.InQuad))
                .Pause();

            // Warm up every tween before the popup enters the pool.
            _animation.Complete();
            _animation.Rewind();
        }

        private void Hide()
        {
            _animation?.Rewind();
            IsActive = false;
            _target = null;
            _spawnId = 0;
            _riseOffset = 0f;
            _rect.localScale = Vector3.one * 0.75f;
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
