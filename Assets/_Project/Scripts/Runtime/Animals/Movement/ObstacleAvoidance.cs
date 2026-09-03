using UnityEngine;

namespace ZooWorld.Animals.Movement
{
    public sealed class ObstacleAvoidance
    {
        private readonly Vector3[] _normals = new Vector3[4];
        private int _count;

        public bool HasContacts => _count > 0;

        public bool AddContact(Vector3 normal)
        {
            if (Mathf.Abs(normal.y) >= 0.7f)
                return false;

            normal.y = 0f;

            if (normal.sqrMagnitude < 0.0001f)
                return false;

            normal.Normalize();

            for (int i = 0; i < _count; i++)
            {
                if (Vector3.Dot(_normals[i], normal) > 0.99f)
                    return true;
            }

            if (_count < _normals.Length)
                _normals[_count++] = normal;

            return true;
        }

        public Vector3 ResolveDirection(Vector3 direction)
        {
            if (!HasContacts)
                return direction;

            for (int i = 0; i < _count; i++)
            {
                if (Vector3.Dot(direction, _normals[i]) < 0f)
                    direction = Vector3.Reflect(direction, _normals[i]);
            }

            if (direction.sqrMagnitude > 0.0001f && IsClear(direction))
                return direction.normalized;

            // In a narrow corner, a reflection off one wall can point back into another.
            for (int i = 0; i < _count; i++)
            {
                if (IsClear(_normals[i]))
                    return _normals[i];

                Vector3 tangent = Vector3.Cross(Vector3.up, _normals[i]);

                if (IsClear(tangent))
                    return tangent;

                if (IsClear(-tangent))
                    return -tangent;
            }

            return Vector3.zero;
        }

        public void Clear()
        {
            _count = 0;
        }

        private bool IsClear(Vector3 direction)
        {
            for (int i = 0; i < _count; i++)
            {
                if (Vector3.Dot(direction, _normals[i]) < -0.0001f)
                    return false;
            }

            return true;
        }
    }
}
