using UnityEngine;
using System.Collections.Generic;

namespace FootballTraining.Utility
{
    /// <summary>
    /// Procedurally generates yard lines, hash marks, and gap label markers on the field.
    /// Called at scene start so no static meshes need to be pre-built.
    /// </summary>
    public class FieldLineGenerator : MonoBehaviour
    {
        [Header("Field Dimensions (yards)")]
        [SerializeField] private float _fieldWidth  = 53.3f;
        [SerializeField] private float _yardLength  = 1f;     // 1 Unity unit = 1 yard
        [SerializeField] private int   _yardLines   = 20;     // lines to show on either side of LOS

        [Header("Materials")]
        [SerializeField] private Material _yardLineMaterial;
        [SerializeField] private Material _hashMarkMaterial;
        [SerializeField] private Material _losLineMaterial;

        [Header("Hash Marks")]
        [SerializeField] private float _hashInset  = 18.5f;   // yards from sideline
        [SerializeField] private float _hashLength = 2f;

        private readonly List<GameObject> _generated = new();

        private void Start() => GenerateField();

        public void GenerateField()
        {
            ClearField();
            GenerateYardLines();
            GenerateHashMarks();
        }

        private void GenerateYardLines()
        {
            for (int i = -_yardLines; i <= _yardLines; i++)
            {
                if (i == 0) continue;  // LOS drawn separately
                float z = i * _yardLength;
                var line = CreateLine($"YardLine_{i}", new Vector3(-_fieldWidth / 2f, 0.01f, z),
                                       new Vector3( _fieldWidth / 2f, 0.01f, z), _yardLineMaterial, 0.08f);
                _generated.Add(line);
            }

            // LOS
            var los = CreateLine("LOS", new Vector3(-_fieldWidth / 2f, 0.02f, 0),
                                        new Vector3( _fieldWidth / 2f, 0.02f, 0), _losLineMaterial, 0.15f);
            _generated.Add(los);
        }

        private void GenerateHashMarks()
        {
            float leftHash  = -(_fieldWidth / 2f - _hashInset);
            float rightHash =   _fieldWidth / 2f - _hashInset;

            for (int i = -_yardLines; i <= _yardLines; i++)
            {
                float z = i * _yardLength;
                foreach (float x in new[] { leftHash, rightHash })
                {
                    var mark = CreateLine($"Hash_{i}_{(x < 0 ? "L" : "R")}",
                        new Vector3(x - _hashLength / 2f, 0.015f, z),
                        new Vector3(x + _hashLength / 2f, 0.015f, z),
                        _hashMarkMaterial, 0.05f);
                    _generated.Add(mark);
                }
            }
        }

        private static GameObject CreateLine(string name, Vector3 start, Vector3 end,
                                             Material mat, float width)
        {
            var go = new GameObject(name);
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            lr.startWidth = lr.endWidth = width;
            if (mat != null) lr.material = mat;
            lr.useWorldSpace = true;
            return go;
        }

        private void ClearField()
        {
            foreach (var go in _generated) if (go != null) Destroy(go);
            _generated.Clear();
        }
    }
}
