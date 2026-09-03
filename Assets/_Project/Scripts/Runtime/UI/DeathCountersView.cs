using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ZooWorld.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Zoo World/UI/Death Counters View")]
    public sealed class DeathCountersView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _preyCounter;
        [SerializeField] private TextMeshProUGUI _predatorCounter;

        private readonly char[] _buffer = new char[32];
        private RectTransform _preyRect;
        private RectTransform _predatorRect;
        private Vector3 _preyScale;
        private Vector3 _predatorScale;
        private Sequence _preyPulse;
        private Sequence _predatorPulse;
        private int _lastPreyDeaths = -1;
        private int _lastPredatorDeaths = -1;

        public void Initialize()
        {
            if (_preyPulse != null || _predatorPulse != null)
                throw new InvalidOperationException("Death counters are already initialized.");

            if (!isActiveAndEnabled || _preyCounter == null || _predatorCounter == null ||
                _preyCounter == _predatorCounter)
            {
                throw new InvalidOperationException("Death counters: enable the view and assign two different UI text components.");
            }

            WarmUp(_preyCounter, "Prey deaths: 0123456789");
            WarmUp(_predatorCounter, "Predator deaths: 0123456789");

            _preyRect = _preyCounter.rectTransform;
            _predatorRect = _predatorCounter.rectTransform;
            _preyScale = _preyRect.localScale;
            _predatorScale = _predatorRect.localScale;
            _preyPulse = CreatePulse(_preyRect, _preyScale);
            _predatorPulse = CreatePulse(_predatorRect, _predatorScale);
            _lastPreyDeaths = -1;
            _lastPredatorDeaths = -1;
        }

        public void Show(int preyDeaths, int predatorDeaths)
        {
            if (preyDeaths != _lastPreyDeaths)
            {
                SetCounter(_preyCounter, "Prey deaths: ", preyDeaths);

                if (_lastPreyDeaths >= 0)
                    _preyPulse.Restart();

                _lastPreyDeaths = preyDeaths;
            }

            if (predatorDeaths != _lastPredatorDeaths)
            {
                SetCounter(_predatorCounter, "Predator deaths: ", predatorDeaths);

                if (_lastPredatorDeaths >= 0)
                    _predatorPulse.Restart();

                _lastPredatorDeaths = predatorDeaths;
            }
        }

        public void Tick(float deltaTime)
        {
            _preyPulse.ManualUpdate(deltaTime, deltaTime);
            _predatorPulse.ManualUpdate(deltaTime, deltaTime);
        }

        public void DisposeAnimations()
        {
            _preyPulse?.Kill();
            _predatorPulse?.Kill();
            _preyPulse = null;
            _predatorPulse = null;

            if (_preyRect != null)
                _preyRect.localScale = _preyScale;

            if (_predatorRect != null)
                _predatorRect.localScale = _predatorScale;
        }

        private void OnDestroy()
        {
            DisposeAnimations();
        }

        private static Sequence CreatePulse(Transform target, Vector3 baseScale)
        {
            Vector3 peakScale = baseScale * 1.1f;
            Sequence pulse = DOTween.Sequence()
                .SetAutoKill(false)
                .SetRecyclable(false)
                .SetUpdate(UpdateType.Manual)
                .Append(target.DOScale(peakScale, 0.1f).From(baseScale, false).SetEase(Ease.OutQuad))
                .Append(target.DOScale(baseScale, 0.16f).From(peakScale, false).SetEase(Ease.InOutQuad))
                .Pause();

            pulse.Complete();
            pulse.Rewind();
            return pulse;
        }

        private void SetCounter(TextMeshProUGUI label, string prefix, int value)
        {
            prefix.CopyTo(0, _buffer, 0, prefix.Length);
            int length = prefix.Length;

            do
            {
                _buffer[length++] = (char)('0' + value % 10);
                value /= 10;
            } while (value > 0);

            Array.Reverse(_buffer, prefix.Length, length - prefix.Length);
            label.SetCharArray(_buffer, 0, length);
        }

        private static void WarmUp(TextMeshProUGUI label, string text)
        {
            if (!label.isActiveAndEnabled || label.canvas == null || !label.canvas.isActiveAndEnabled || label.font == null)
                throw new InvalidOperationException("Death counters: place active text components under a Canvas and assign a TMP font.");

            label.raycastTarget = false;
            label.SetText(text);
            label.ForceMeshUpdate();
        }
    }
}
