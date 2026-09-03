using System;
using UnityEngine;

namespace ZooWorld.World
{
    public sealed class WorldBoundsProvider
    {
        private readonly Camera _camera;
        private readonly Transform _cameraTransform;
        private readonly float _padding;

        private Vector3 _cameraPosition;
        private Quaternion _cameraRotation;
        private float _orthographicSize;
        private float _aspect;
        private float _nearClip;
        private float _farClip;

        private Vector3 _center;
        private Vector3 _right;
        private Vector3 _up;
        private float _halfWidth;
        private float _halfHeight;
        private bool _initialized;

        public float GroundHeight { get; }

        public WorldBoundsProvider(Camera camera, float groundHeight, float padding)
        {
            _camera = camera;
            _cameraTransform = camera != null ? camera.transform : null;
            GroundHeight = groundHeight;
            _padding = padding;
        }

        public void Initialize()
        {
            _initialized = false;
            Refresh();
        }

        public void Refresh()
        {
            if (_camera == null)
                throw new InvalidOperationException("World bounds: assign the gameplay camera.");

            if (!_initialized && (!IsFinite(GroundHeight) || !IsFinite(_padding) || _padding < 0f))
            {
                throw new InvalidOperationException(
                    "World bounds: Ground Height must be finite and Padding must be finite and non-negative.");
            }

            Vector3 position = _cameraTransform.position;
            Quaternion rotation = _cameraTransform.rotation;
            float size = _camera.orthographicSize;
            float aspect = _camera.aspect;
            float nearClip = _camera.nearClipPlane;
            float farClip = _camera.farClipPlane;

            if (_initialized && _camera.orthographic &&
                position.Equals(_cameraPosition) && rotation.Equals(_cameraRotation) &&
                size == _orthographicSize && aspect == _aspect &&
                nearClip == _nearClip && farClip == _farClip)
            {
                return;
            }

            if (!_camera.orthographic)
                throw new InvalidOperationException("World bounds: the gameplay camera must be orthographic.");

            if (!IsFinite(size) || size <= 0f || !IsFinite(aspect) || aspect <= 0f ||
                !IsFinite(size * aspect))
            {
                throw new InvalidOperationException(
                    "World bounds: the camera must have a finite positive Orthographic Size and Aspect.");
            }

            Vector3 forward = _cameraTransform.forward;

            if (!(Vector3.Dot(forward, Vector3.down) >= 0.999999f))
            {
                throw new InvalidOperationException(
                    "World bounds: point the gameplay camera straight down (Rotation X = 90). Rotation around the viewing axis is supported.");
            }

            if (!IsFinite(position.x) || !IsFinite(position.y) || !IsFinite(position.z) ||
                position.y <= GroundHeight)
            {
                throw new InvalidOperationException(
                    "World bounds: the camera position must be finite and above Ground Height.");
            }

            float distance = (GroundHeight - position.y) / forward.y;

            if (!IsFinite(nearClip) || !IsFinite(farClip) || nearClip < 0f || farClip <= nearClip ||
                distance <= nearClip || distance >= farClip)
            {
                throw new InvalidOperationException(
                    "World bounds: the ground plane must lie between the camera's Near and Far clipping planes.");
            }

            _center = position + forward * distance;
            _center.y = GroundHeight;
            _right = _cameraTransform.right;
            _right.y = 0f;
            _right.Normalize();
            _up = _cameraTransform.up;
            _up.y = 0f;
            _up.Normalize();
            _halfWidth = size * aspect;
            _halfHeight = size;

            _cameraPosition = position;
            _cameraRotation = rotation;
            _orthographicSize = size;
            _aspect = aspect;
            _nearClip = nearClip;
            _farClip = farClip;
            _initialized = true;
        }

        public void ValidateRadius(float radius)
        {
            RequireInitialized();

            if (!IsFinite(radius) || radius <= 0f)
                throw new ArgumentOutOfRangeException(nameof(radius), "Animal radius must be finite and positive.");

            if (_halfWidth <= _padding + radius || _halfHeight <= _padding + radius)
            {
                throw new InvalidOperationException(
                    "World bounds: the camera view is too small for the animal radius and boundary padding. Increase Orthographic Size or reduce Padding.");
            }
        }

        public bool TryGetRandomPosition(float radius, out Vector3 position)
        {
            RequireInitialized();
            position = default;
            float availableWidth = _halfWidth - _padding - radius;
            float availableHeight = _halfHeight - _padding - radius;

            if (!IsFinite(radius) || radius <= 0f || availableWidth <= 0f || availableHeight <= 0f)
                return false;

            float horizontal = UnityEngine.Random.Range(-availableWidth, availableWidth);
            float vertical = UnityEngine.Random.Range(-availableHeight, availableHeight);
            position = _center + _right * horizontal + _up * vertical;
            position.y = GroundHeight;
            return true;
        }

        public bool TryGetReturnDirection(Vector3 position, float radius, out Vector3 direction)
        {
            RequireInitialized();
            direction = default;
            Vector3 offset = position - _center;
            offset.y = 0f;

            // Camera-local axes keep the boundary correct when the top-down camera is rotated.
            float horizontal = Vector3.Dot(offset, _right);
            float vertical = Vector3.Dot(offset, _up);

            if (Mathf.Abs(horizontal) <= _halfWidth - _padding - radius &&
                Mathf.Abs(vertical) <= _halfHeight - _padding - radius)
            {
                return false;
            }

            float squaredDistance = offset.sqrMagnitude;

            if (squaredDistance < 0.000001f)
                return false;

            direction = -offset / Mathf.Sqrt(squaredDistance);
            return true;
        }

        private void RequireInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException("World bounds: call Initialize before requesting positions or directions.");
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
