using System;
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

        public void Initialize()
        {
            if (!isActiveAndEnabled || _preyCounter == null || _predatorCounter == null ||
                _preyCounter == _predatorCounter)
            {
                throw new InvalidOperationException("Death counters: enable the view and assign two different UI text components.");
            }

            WarmUp(_preyCounter, "Prey deaths: 0123456789");
            WarmUp(_predatorCounter, "Predator deaths: 0123456789");
        }

        public void Show(int preyDeaths, int predatorDeaths)
        {
            SetCounter(_preyCounter, "Prey deaths: ", preyDeaths);
            SetCounter(_predatorCounter, "Predator deaths: ", predatorDeaths);
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
